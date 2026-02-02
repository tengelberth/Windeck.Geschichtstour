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
    private readonly HttpClient _httpClient;
    private readonly AppUrlOptions _appUrlOptions;

    /// <summary>
    /// Initialisiert den API-Client mit der zentralen URL-Konfiguration.
    /// </summary>
    /// <param name="appUrlOptions">Konfigurierte Backend- und Public-URLs.</param>
    public ApiClient(AppUrlOptions appUrlOptions)
    {
        _appUrlOptions = appUrlOptions;
        _httpClient = new HttpClient
        {
            BaseAddress = _appUrlOptions.BackendBaseUri,
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    /// <summary>
    /// Laedt alle Stationen und normalisiert enthaltene Medien-URLs.
    /// </summary>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Liste mit Stationen; bei Fehlern eine leere Liste.</returns>
    public async Task<List<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetWithRetryAsync(
            endpoint: "api/stations",
            typeInfo: ApiJsonContext.Default.ListStationDto,
            cancellationToken: cancellationToken);

        var stations = result ?? new List<StationDto>();
        foreach (var station in stations)
            NormalizeStationMediaUrls(station);

        return stations;
    }

    /// <summary>
    /// Laedt eine Station ueber ihren Stationcode.
    /// </summary>
    /// <param name="code">Stationcode aus QR-Code oder Deeplink.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Gefundene Station oder <c>null</c>, wenn keine Daten vorliegen.</returns>
    public async Task<StationDto?> GetStationByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code darf nicht leer sein.", nameof(code));

        var endpoint = $"api/stations/by-code/{Uri.EscapeDataString(code.Trim())}";

        var station = await GetWithRetryAsync(
            endpoint: endpoint,
            typeInfo: ApiJsonContext.Default.StationDto,
            cancellationToken: cancellationToken);

        if (station != null)
            NormalizeStationMediaUrls(station);

        return station;
    }

    /// <summary>
    /// Laedt alle Touren aus dem Backend.
    /// </summary>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Liste mit Touren; bei Fehlern eine leere Liste.</returns>
    public async Task<List<TourDto>> GetToursAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetWithRetryAsync(
            endpoint: "api/tours",
            typeInfo: ApiJsonContext.Default.ListTourDto,
            cancellationToken: cancellationToken);

        return result ?? new List<TourDto>();
    }

    /// <summary>
    /// Laedt eine Tour ueber ihre ID.
    /// </summary>
    /// <param name="id">ID der Tour.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Geladene Tour oder <c>null</c>, wenn sie nicht verfuegbar ist.</returns>
    public async Task<TourDto?> GetTourByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/tours/{id}";

        return await GetWithRetryAsync(
            endpoint: endpoint,
            typeInfo: ApiJsonContext.Default.TourDto,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Fuehrt einen GET-Request mit Retry-Dialogen und robuster Fehlerbehandlung aus.
    /// </summary>
    /// <typeparam name="T">Erwarteter Rueckgabetyp der API-Antwort.</typeparam>
    /// <param name="endpoint">Relativer API-Endpunkt.</param>
    /// <param name="typeInfo">JSON-TypeInfo aus dem Source Generator.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der laufenden Anfrage.</param>
    /// <returns>Deserialisiertes Ergebnis oder <c>null</c> bei Fehlern/Abbruch.</returns>
    private async Task<T?> GetWithRetryAsync<T>(
        string endpoint,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                var retry = await ShowRetryDialogAsync(
                    title: "Kein Internet",
                    message: "Du bist offline. Bitte pruefe deine Verbindung und versuche es erneut.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;

                continue;
            }

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return default;

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

                return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var retry = await ShowRetryDialogAsync(
                    title: "Zeitueberschreitung",
                    message: "Die Anfrage hat zu lange gedauert. Moechtest du es erneut versuchen?",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;
            }
            catch (HttpRequestException)
            {
                var retry = await ShowRetryDialogAsync(
                    title: "Verbindung fehlgeschlagen",
                    message: "Der Server ist momentan nicht erreichbar. Bitte versuche es spaeter erneut.",
                    accept: "Wiederholen",
                    cancel: "Abbrechen");

                if (!retry)
                    return default;
            }
            catch (Exception)
            {
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
    /// Ergaenzt relative Medien-URLs einer Station um die konfigurierte Backend-Basis-URL.
    /// </summary>
    /// <param name="station">Station mit zu normalisierenden Medien.</param>
    private void NormalizeStationMediaUrls(StationDto station)
    {
        foreach (var mediaItem in station.MediaItems)
        {
            if (string.IsNullOrWhiteSpace(mediaItem.Url))
            {
                mediaItem.FullUrl = string.Empty;
                continue;
            }

            if (Uri.TryCreate(mediaItem.Url, UriKind.Absolute, out var absoluteUri))
            {
                mediaItem.FullUrl = absoluteUri.ToString();
                continue;
            }

            mediaItem.FullUrl = new Uri(_appUrlOptions.BackendBaseUri, mediaItem.Url.TrimStart('/')).ToString();
        }
    }
}
