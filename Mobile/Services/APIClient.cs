using System.Net;
using System.Net.Http.Json;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;

namespace Windeck.Geschichtstour.Mobile.Services;

/// <summary>
/// ApiClient kapselt alle HTTP-Aufrufe an dein Backend.
///
/// Features:
/// ✅ Zentraler Internet-Check (Offline-Erkennung)
/// ✅ Nutzerhinweis bei fehlendem Internet
/// ✅ "Wiederholen / Abbrechen" direkt im API-Layer
/// ✅ Release-sichere JSON-Deserialisierung via SourceGenerator (ApiJsonContext)
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Basis-URL deines Backends (Produktivsystem).
    /// </summary>
    private const string BaseUrl = "https://geschichtstour-backend.azurewebsites.net/";

    /// <summary>
    /// Erstellt einen ApiClient mit HttpClient und setzt die BaseAddress.
    /// </summary>
    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    /// <summary>
    /// Lädt alle Stationen vom Backend.
    /// GET /api/stations
    ///
    /// Rückgabe:
    /// - Liste von StationDto
    /// - bei Abbruch/Fehler: leere Liste
    /// </summary>
    public async Task<List<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetWithRetryAsync(
            endpoint: "api/stations",
            typeInfo: ApiJsonContext.Default.ListStationDto,
            cancellationToken: cancellationToken);

        return result ?? new List<StationDto>();
    }

    /// <summary>
    /// Lädt eine Station anhand ihres Codes.
    /// GET /api/stations/by-code/{code}
    ///
    /// Rückgabe:
    /// - StationDto wenn gefunden
    /// - null bei Abbruch/Fehler/nicht gefunden
    /// </summary>
    public async Task<StationDto?> GetStationByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code darf nicht leer sein.", nameof(code));

        var endpoint = $"api/stations/by-code/{Uri.EscapeDataString(code.Trim())}";

        return await GetWithRetryAsync(
            endpoint: endpoint,
            typeInfo: ApiJsonContext.Default.StationDto,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lädt alle Touren vom Backend.
    /// GET /api/tours
    ///
    /// Rückgabe:
    /// - Liste von TourDto
    /// - bei Abbruch/Fehler: leere Liste
    /// </summary>
    public async Task<List<TourDto>> GetToursAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetWithRetryAsync(
            endpoint: "api/tours",
            typeInfo: ApiJsonContext.Default.ListTourDto,
            cancellationToken: cancellationToken);

        return result ?? new List<TourDto>();
    }

    /// <summary>
    /// Lädt eine Tour anhand ihrer ID.
    /// GET /api/tours/{id}
    ///
    /// Rückgabe:
    /// - TourDto wenn gefunden
    /// - null bei Abbruch/Fehler/nicht gefunden
    /// </summary>
    public async Task<TourDto?> GetTourByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/tours/{id}";

        return await GetWithRetryAsync(
            endpoint: endpoint,
            typeInfo: ApiJsonContext.Default.TourDto,
            cancellationToken: cancellationToken);
    }

    // ------------------------------------------------------------------------
    // ZENTRALE HELPER (Internet-Check + Retry + JSON parse)
    // ------------------------------------------------------------------------

    /// <summary>
    /// Führt einen GET Request aus, prüft vorher Internet und bietet Wiederholen/Abbrechen an.
    /// Deserialisiert Release-safe über SourceGenerator typeInfo.
    ///
    /// Rückgabe:
    /// - Objekt T wenn erfolgreich
    /// - null wenn abgebrochen oder nicht möglich
    /// </summary>
    private async Task<T?> GetWithRetryAsync<T>(
        string endpoint,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            // 1) Offline-Check
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                var retry = await ShowRetryDialogAsync(
                    title: "Kein Internet",
                    message: "Du bist offline. Bitte überprüfe deine Verbindung und versuche es erneut.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;

                continue;
            }

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

                // 2) 404 sauber behandeln (kein Retry notwendig, aber möglich)
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Optional: Du könntest hier auch ohne Dialog einfach null zurückgeben.
                    return default;
                }

                // 3) Falls Fehlercodes: Nutzer kann Wiederholen
                if (!response.IsSuccessStatusCode)
                {
                    var retry = await ShowRetryDialogAsync(
                        title: "Serverfehler",
                        message: $"Die Daten konnten nicht geladen werden. (HTTP {(int)response.StatusCode})",
                        accept: "Wiederholen",
                        cancel: "Abbrechen");

                    if (!retry)
                        return default;

                    continue;
                }

                // 4) JSON lesen (Release-safe via SourceGenerator)
                var data = await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken);
                return data;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout (HttpClient Timeout) -> Wiederholen anbieten
                var retry = await ShowRetryDialogAsync(
                    title: "Zeitüberschreitung",
                    message: "Die Anfrage hat zu lange gedauert. Möchtest du es erneut versuchen?",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;
            }
            catch (HttpRequestException)
            {
                // Verbindung/SSL/DNS/Server nicht erreichbar
                var retry = await ShowRetryDialogAsync(
                    title: "Verbindung fehlgeschlagen",
                    message: "Der Server ist momentan nicht erreichbar. Bitte versuche es später erneut.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;
            }
            catch (Exception)
            {
                // Unbekannter Fehler -> Wiederholen anbieten
                var retry = await ShowRetryDialogAsync(
                    title: "Fehler",
                    message: "Es ist ein unerwarteter Fehler aufgetreten.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;
            }
        }
    }

    /// <summary>
    /// Zeigt einen Dialog an, der dem Nutzer Wiederholen/Abbrechen anbietet.
    /// Gibt true zurück, wenn Wiederholen gewählt wurde.
    /// </summary>
    private static Task<bool> ShowRetryDialogAsync(string title, string message, string accept, string cancel)
    {
        // Accept wird als Button benutzt, Cancel passiert durch "auslaufen".
        return UiNotify.SnackbarRetryAsync(
            message,
            accept,
            seconds: 5
        );
    }

}
