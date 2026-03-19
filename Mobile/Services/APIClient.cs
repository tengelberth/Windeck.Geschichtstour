using System.Net;
using System.Net.Http.Json;
using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;

namespace Windeck.Geschichtstour.Mobile.Services;

/// <summary>
/// Kapselt alle HTTP-Aufrufe zur Backend-API inkl. Retry- und Offline-Handling.
/// </summary>
public class ApiClient
{
    private static readonly TimeSpan SilentTransientRetryWindow = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan SilentTransientRetryDelay = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan StationsCacheTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan ToursCacheTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan StationDetailCacheTtl = TimeSpan.FromDays(7);
    private static readonly TimeSpan TourDetailCacheTtl = TimeSpan.FromDays(7);

    private readonly HttpClient _httpClient;
    private readonly AppUrlOptions _appUrlOptions;
    private readonly JsonCacheService _cacheService;

    /// <summary>
    /// Initialisiert den API-Client mit der zentralen URL-Konfiguration.
    /// </summary>
    /// <param name="appUrlOptions">Konfigurierte Backend- und Public-URLs.</param>
    /// <param name="cacheService">Plattformneutraler JSON-Cache für Listen und bereits geladene Details.</param>
    public ApiClient(AppUrlOptions appUrlOptions, JsonCacheService cacheService)
    {
        _appUrlOptions = appUrlOptions;
        _cacheService = cacheService;
        _httpClient = new HttpClient
        {
            BaseAddress = _appUrlOptions.BackendBaseUri,
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    /// <summary>
    /// Lädt alle Stationen und normalisiert enthaltene Medien-URLs.
    /// </summary>
    /// <param name="allowUserRetryUi">Steuert, ob nach dem stillen Retry-Fenster sichtbare Retry-Hinweise erscheinen dürfen.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Liste mit Stationen; bei Fehlern eine leere Liste.</returns>
    public async Task<List<StationDto>> GetStationsAsync(bool allowUserRetryUi = true, CancellationToken cancellationToken = default)
    {
        List<StationDto>? result = await GetWithRetryAsync(
            endpoint: "api/stations",
            typeInfo: ApiJsonContext.Default.ListStationDto,
            allowUserRetryUi: allowUserRetryUi,
            cancellationToken: cancellationToken);

        List<StationDto> stations = result ?? new List<StationDto>();
        foreach (StationDto? station in stations)
        {
            NormalizeStationMediaUrls(station);
        }

        if (result != null)
        {
            await _cacheService.SetAsync(GetStationsCacheKey(), stations, ApiJsonContext.Default.ListStationDto, cancellationToken);
        }

        return stations;
    }

    /// <summary>
    /// Liefert eine zuvor gespeicherte Stationsliste, falls sie noch im Cache vorhanden ist.
    /// </summary>
    public async Task<List<StationDto>?> TryGetCachedStationsAsync(CancellationToken cancellationToken = default)
    {
        List<StationDto>? stations = await _cacheService.TryGetAsync(
            GetStationsCacheKey(),
            ApiJsonContext.Default.ListStationDto,
            StationsCacheTtl,
            cancellationToken);

        if (stations == null)
        {
            return null;
        }

        foreach (StationDto station in stations)
        {
            NormalizeStationMediaUrls(station);
        }

        return stations;
    }

    /// <summary>
    /// Lädt eine Station ueber ihren Stationcode.
    /// </summary>
    /// <param name="code">Stationcode aus QR-Code oder Deeplink.</param>
    /// <param name="allowUserRetryUi">Steuert, ob nach dem stillen Retry-Fenster sichtbare Retry-Hinweise erscheinen dürfen.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Gefundene Station oder <c>null</c>, wenn keine Daten vorliegen.</returns>
    public async Task<StationDto?> GetStationByCodeAsync(string code, bool allowUserRetryUi = true, CancellationToken cancellationToken = default)
    {
        StationFetchResult result = await GetStationByCodeWithStatusAsync(code, allowUserRetryUi, cancellationToken);
        return result.Station;
    }

    /// <summary>
    /// Liefert eine bereits geöffnete Station aus dem Cache, falls sie noch lokal vorhanden ist.
    /// </summary>
    public async Task<StationDto?> TryGetCachedStationByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        StationDto? station = await _cacheService.TryGetAsync(
            GetStationCacheKey(code),
            ApiJsonContext.Default.StationDto,
            StationDetailCacheTtl,
            cancellationToken);

        if (station != null)
        {
            NormalizeStationMediaUrls(station);
        }

        return station;
    }

    /// <summary>
    /// Lädt eine Station ueber ihren Stationcode und liefert zusätzlich zurueck, ob der Code serverseitig nicht existiert.
    /// </summary>
    /// <param name="code">Stationcode aus QR-Code oder Deeplink.</param>
    /// <param name="allowUserRetryUi">Steuert, ob nach dem stillen Retry-Fenster sichtbare Retry-Hinweise erscheinen dürfen.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Fetch-Ergebnis mit Station und NotFound-Kennzeichen.</returns>
    public async Task<StationFetchResult> GetStationByCodeWithStatusAsync(string code, bool allowUserRetryUi = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code darf nicht leer sein.", nameof(code));
        }

        string normalizedCode = code.Trim();
        string endpoint = $"api/stations/by-code/{Uri.EscapeDataString(normalizedCode)}";

        FetchResult<StationDto> result = await GetWithRetryDetailedAsync(
            endpoint: endpoint,
            typeInfo: ApiJsonContext.Default.StationDto,
            allowUserRetryUi: allowUserRetryUi,
            cancellationToken: cancellationToken);

        if (result.Value != null)
        {
            NormalizeStationMediaUrls(result.Value);
            await _cacheService.SetAsync(GetStationCacheKey(normalizedCode), result.Value, ApiJsonContext.Default.StationDto, cancellationToken);
        }

        return new StationFetchResult(result.Value, result.WasNotFound);
    }

    /// <summary>
    /// Lädt alle Touren aus dem Backend.
    /// </summary>
    /// <param name="allowUserRetryUi">Steuert, ob nach dem stillen Retry-Fenster sichtbare Retry-Hinweise erscheinen dürfen.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Liste mit Touren; bei Fehlern eine leere Liste.</returns>
    public async Task<List<TourDto>> GetToursAsync(bool allowUserRetryUi = true, CancellationToken cancellationToken = default)
    {
        List<TourDto>? result = await GetWithRetryAsync(
            endpoint: "api/tours",
            typeInfo: ApiJsonContext.Default.ListTourDto,
            allowUserRetryUi: allowUserRetryUi,
            cancellationToken: cancellationToken);

        List<TourDto> tours = result ?? new List<TourDto>();

        if (result != null)
        {
            await _cacheService.SetAsync(GetToursCacheKey(), tours, ApiJsonContext.Default.ListTourDto, cancellationToken);
        }

        return tours;
    }

    /// <summary>
    /// Liefert eine zuvor gespeicherte Tourenliste, falls sie noch im Cache vorhanden ist.
    /// </summary>
    public Task<List<TourDto>?> TryGetCachedToursAsync(CancellationToken cancellationToken = default)
    {
        return _cacheService.TryGetAsync(
            GetToursCacheKey(),
            ApiJsonContext.Default.ListTourDto,
            ToursCacheTtl,
            cancellationToken);
    }

    /// <summary>
    /// Lädt eine Tour ueber ihre ID.
    /// </summary>
    /// <param name="id">ID der Tour.</param>
    /// <param name="allowUserRetryUi">Steuert, ob nach dem stillen Retry-Fenster sichtbare Retry-Hinweise erscheinen dürfen.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Geladene Tour oder <c>null</c>, wenn sie nicht verfuegbar ist.</returns>
    public async Task<TourDto?> GetTourByIdAsync(int id, bool allowUserRetryUi = true, CancellationToken cancellationToken = default)
    {
        string endpoint = $"api/tours/{id}";

        TourDto? tour = await GetWithRetryAsync(
            endpoint: endpoint,
            typeInfo: ApiJsonContext.Default.TourDto,
            allowUserRetryUi: allowUserRetryUi,
            cancellationToken: cancellationToken);

        if (tour != null)
        {
            await _cacheService.SetAsync(GetTourCacheKey(id), tour, ApiJsonContext.Default.TourDto, cancellationToken);
        }

        return tour;
    }

    /// <summary>
    /// Liefert eine bereits geöffnete Tour aus dem Cache, falls sie noch lokal vorhanden ist.
    /// </summary>
    public Task<TourDto?> TryGetCachedTourByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _cacheService.TryGetAsync(
            GetTourCacheKey(id),
            ApiJsonContext.Default.TourDto,
            TourDetailCacheTtl,
            cancellationToken);
    }

    /// <summary>
    /// Führt einen stillen Bereitschaftscheck aus, um den Backend- und Datenbankpfad bei App-Start vorzuwärmen.
    /// </summary>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns><c>true</c>, wenn der Endpunkt erfolgreich antwortet; sonst <c>false</c>.</returns>
    public async Task<bool> PingReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("readyz", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Fuehrt einen GET-Request mit Retry-Dialogen und robuster Fehlerbehandlung aus.
    /// </summary>
    /// <typeparam name="T">Erwarteter Rueckgabetyp der API-Antwort.</typeparam>
    /// <param name="endpoint">Relativer API-Endpunkt.</param>
    /// <param name="typeInfo">JSON-TypeInfo aus dem Source Generator.</param>
    /// <param name="allowUserRetryUi">Steuert, ob nach dem stillen Retry-Fenster sichtbare Retry-Hinweise erscheinen dürfen.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Deserialisiertes Ergebnis oder <c>null</c> bei Fehlern/Abbruch.</returns>
    private async Task<T?> GetWithRetryAsync<T>(
        string endpoint,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
        bool allowUserRetryUi,
        CancellationToken cancellationToken)
    {
        FetchResult<T> result = await GetWithRetryDetailedAsync(endpoint, typeInfo, allowUserRetryUi, cancellationToken);
        return result.Value;
    }

    /// <summary>
    /// Fuehrt einen GET-Request aus und liefert zusätzlich zurück, ob die Ressource serverseitig nicht gefunden wurde.
    /// </summary>
    private async Task<FetchResult<T>> GetWithRetryDetailedAsync<T>(
        string endpoint,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
        bool allowUserRetryUi,
        CancellationToken cancellationToken)
    {
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;

        while (true)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                if (!allowUserRetryUi)
                {
                    return new FetchResult<T>(default, false);
                }

                bool retry = await ShowRetryDialogAsync(
                    title: "Kein Internet",
                    message: "Du bist offline. Bitte pruefe deine Verbindung und versuche es erneut.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                {
                    return new FetchResult<T>(default, false);
                }

                continue;
            }

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new FetchResult<T>(default, true);
                }

                if (!response.IsSuccessStatusCode)
                {
                    if (IsTransientStatusCode(response.StatusCode) &&
                        await TrySilentTransientRetryAsync(startedAt, cancellationToken))
                    {
                        continue;
                    }

                    if (!allowUserRetryUi)
                    {
                        return new FetchResult<T>(default, false);
                    }

                    bool retry = await ShowRetryDialogAsync(
                        title: "Serverfehler",
                        message: $"Die Daten konnten nicht geladen werden. (HTTP {(int)response.StatusCode})",
                        accept: "Wiederholen",
                        cancel: "Abbrechen");

                    if (!retry)
                    {
                        return new FetchResult<T>(default, false);
                    }

                    continue;
                }

                T? value = await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken);
                return new FetchResult<T>(value, false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (await TrySilentTransientRetryAsync(startedAt, cancellationToken))
                {
                    continue;
                }

                if (!allowUserRetryUi)
                {
                    return new FetchResult<T>(default, false);
                }

                bool retry = await ShowRetryDialogAsync(
                    title: "Zeitüberschreitung",
                    message: "Die Anfrage hat zu lange gedauert. Möchtest du es erneut versuchen?",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                {
                    return new FetchResult<T>(default, false);
                }
            }
            catch (HttpRequestException)
            {
                if (await TrySilentTransientRetryAsync(startedAt, cancellationToken))
                {
                    continue;
                }

                if (!allowUserRetryUi)
                {
                    return new FetchResult<T>(default, false);
                }

                bool retry = await ShowRetryDialogAsync(
                    title: "Verbindung fehlgeschlagen",
                    message: "Der Server ist momentan nicht erreichbar. Bitte versuche es später erneut.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                {
                    return new FetchResult<T>(default, false);
                }
            }
            catch (Exception)
            {
                if (await TrySilentTransientRetryAsync(startedAt, cancellationToken))
                {
                    continue;
                }

                if (!allowUserRetryUi)
                {
                    return new FetchResult<T>(default, false);
                }

                bool retry = await ShowRetryDialogAsync(
                    title: "Fehler",
                    message: "Es ist ein unerwarteter Fehler aufgetreten.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                {
                    return new FetchResult<T>(default, false);
                }
            }
        }
    }

    /// <summary>
    /// Gewährt bei kalten Backend-Starts ein zusammenhängendes stilles Retry-Fenster, bevor ein Fehler angezeigt wird.
    /// </summary>
    private static async Task<bool> TrySilentTransientRetryAsync(DateTimeOffset startedAt, CancellationToken cancellationToken)
    {
        TimeSpan elapsed = DateTimeOffset.UtcNow - startedAt;
        if (elapsed >= SilentTransientRetryWindow)
        {
            return false;
        }

        await Task.Delay(SilentTransientRetryDelay, cancellationToken);
        return true;
    }

    /// <summary>
    /// Kennzeichnet HTTP-Statuscodes, die bei einem kalten Backend typischerweise nur voruebergehend sind.
    /// </summary>
    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    /// <summary>
    /// Zeigt einen Retry-Hinweis mit Wiederholen/Abbrechen an.
    /// </summary>
    /// <param name="title">Titel des Hinweises.</param>
    /// <param name="message">Fehlermeldung fuer den Nutzer.</param>
    /// <param name="accept">Text der Aktion zum Wiederholen.</param>
    /// <param name="cancel">Text der Aktion zum Abbrechen.</param>
    /// <returns><c>true</c>, wenn erneut versucht werden soll; sonst <c>false</c>.</returns>
    private static Task<bool> ShowRetryDialogAsync(string title, string message, string accept, string cancel)
    {
        return UiNotify.SnackbarRetryAsync(
            message,
            accept,
            seconds: 5
        );
    }

    /// <summary>
    /// Ergänzt relative Medien-URLs einer Station um die konfigurierte Backend-Basis-URL.
    /// </summary>
    /// <param name="station">Station mit zu normalisierenden Medien.</param>
    private void NormalizeStationMediaUrls(StationDto station)
    {
        foreach (MediaItemDto mediaItem in station.MediaItems)
        {
            if (string.IsNullOrWhiteSpace(mediaItem.Url))
            {
                mediaItem.FullUrl = string.Empty;
                continue;
            }

            mediaItem.FullUrl = new Uri(_appUrlOptions.BackendBaseUri, mediaItem.Url.TrimStart('/')).ToString();
        }
    }

    private static string GetStationsCacheKey() => "stations_all";
    private static string GetToursCacheKey() => "tours_all";
    private static string GetStationCacheKey(string code) => $"station_{code.Trim().ToUpperInvariant()}";
    private static string GetTourCacheKey(int id) => $"tour_{id}";

    /// <summary>
    /// Detailergebnis einer Stationenabfrage inklusive NotFound-Kennzeichnung.
    /// </summary>
    public readonly record struct StationFetchResult(StationDto? Station, bool WasNotFound);

    /// <summary>
    /// Generisches Fetch-Ergebnis inklusive serverseitigem NotFound-Zustand.
    /// </summary>
    private readonly record struct FetchResult<T>(T? Value, bool WasNotFound);
}
