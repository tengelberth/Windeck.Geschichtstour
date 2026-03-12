using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using System.Globalization;
using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Verwaltet Stationsdetails, Medienanzeige und externe Aktionen wie Teilen oder Navigation.
/// </summary>
public class StationContentViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly AppUrlOptions _appUrlOptions;

    private StationDto? _station;
    private string? _loadedStationCode;
    private List<MediaItemDto> _imageMediaItems = new();
    private int _mediaPosition;
    private string _longDescriptionHtml = BuildLongDescriptionHtml(string.Empty);
    private double _longDescriptionHeight = 1800;

    public IReadOnlyList<MediaItemDto> ImageMediaItems => _imageMediaItems;
    public bool HasImages => _imageMediaItems.Count > 0;
    public bool HasMultipleImages => _imageMediaItems.Count > 1;
    public string CurrentMediaIndicator => _imageMediaItems.Count == 0
        ? string.Empty
        : $"{MediaPosition + 1}/{_imageMediaItems.Count}";

    public StationDto? Station
    {
        get => _station;
        set
        {
            if (SetProperty(ref _station, value))
            {
                // Abhängige Properties benachrichtigen
                OnPropertyChanged(nameof(HasStation));
                LongDescriptionHtml = BuildLongDescriptionHtml(_station?.LongDescription ?? string.Empty);
                LongDescriptionHeight = 1800;
                // Zusätzliche Logik nach der Änderung
                RebuildImageMediaItems();
                // Commands neu bewerten
                OpenInMapsCommand.ChangeCanExecute();
            }
        }
    }

    public bool HasStation => Station != null;

    public string LongDescriptionHtml
    {
        get => _longDescriptionHtml;
        private set => SetProperty(ref _longDescriptionHtml, value);
    }

    public double LongDescriptionHeight
    {
        get => _longDescriptionHeight;
        set => SetProperty(ref _longDescriptionHeight, value);
    }

    // Carousel Position
    public int MediaPosition
    {
        get => _mediaPosition;
        set
        {
            if (SetProperty(ref _mediaPosition, value))
            {
                OnPropertyChanged(nameof(CurrentMediaIndicator));
            }
        }
    }

    public Command OpenInMapsCommand { get; }
    public Command OpenInKomootCommand { get; }
    public Command ShareStationCommand { get; }
    public Command<MediaItemDto> OpenMediaOverlayCommand { get; }

    /// <summary>
    /// Initialisiert das ViewModel fuer Stationsdetails inkl. Mediensteuerung.
    /// </summary>
    /// <param name="apiClient">API-Service zum Laden von Stationsdaten.</param>
    /// <param name="appUrlOptions">URL-Konfiguration fuer Share-Links.</param>
    public StationContentViewModel(ApiClient apiClient, AppUrlOptions appUrlOptions)
    {
        _apiClient = apiClient;
        _appUrlOptions = appUrlOptions;

        OpenInMapsCommand = new Command(async () => await OpenInMapsAsync(), () => Station != null);
        OpenInKomootCommand = new Command(async () => await OpenInKomootAsync());
        ShareStationCommand = new Command(async () => await ShareStationAsync());

        OpenMediaOverlayCommand = new Command<MediaItemDto>(async item =>
        {
            if (item == null || string.IsNullOrWhiteSpace(item.FullUrl))
                return;
            var popup = new MediaPreviewPopup(item.FullUrl);
            await Application.Current.MainPage.ShowPopupAsync(popup);
        });
    }

    /// <summary>
    /// Laedt Stationsdetails fuer den uebergebenen Code und aktualisiert die Medienliste.
    /// </summary>
    /// <param name="code">Stationscode aus QR-Code oder Deeplink.</param>
    /// <returns>Asynchroner Vorgang zum Laden und Aktualisieren des Zustands.</returns>
    public async Task LoadByCodeAsync(string code)
    {
        if (IsBusy) return;

        var normalizedCode = code?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCode))
            return;

        if (Station is not null && string.Equals(_loadedStationCode, normalizedCode, StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            IsBusy = true;
            Station = null;

            var station = await _apiClient.GetStationByCodeAsync(normalizedCode);
            Station = station;
            _loadedStationCode = station?.Code;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Station mit Code {normalizedCode}: {ex}");
            Station = null;
            _loadedStationCode = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Erstellt einen oeffentlichen Share-Link zur aktuellen Station.
    /// </summary>
    /// <returns>Asynchroner Vorgang zum Oeffnen des systemweiten Share-Dialogs.</returns>
    public async Task ShareStationAsync()
    {
        if (Station == null)
            return;

        var url = new Uri(_appUrlOptions.PublicBaseUri, $"station?code={Uri.EscapeDataString(Station.Code)}").ToString();

        await Share.RequestAsync(new ShareTextRequest
        {
            Title = Station.Title,
            Text = "Schau dir diese Station an:",
            Uri = url
        });
    }

    /// <summary>
    /// Filtert, sortiert und publiziert die Bildmedien fuer das Carousel.
    /// </summary>
    private void RebuildImageMediaItems()
    {
        _imageMediaItems =
            Station?.MediaItems?
                .Where(m => m.MediaType.Equals("Image", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.SortOrder)
                .ToList()
            ?? new List<MediaItemDto>();

        OnPropertyChanged(nameof(ImageMediaItems));
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(HasMultipleImages));
        OnPropertyChanged(nameof(CurrentMediaIndicator));
        MediaPosition = _imageMediaItems.Count > 0 ? Math.Clamp(MediaPosition, 0, _imageMediaItems.Count - 1) : 0;
    }

    /// <summary>
    /// Baut ein vollständiges HTML-Dokument für die Beschreibung inklusive Styling,
    /// Link-Weiterleitung und Höhen-Callback an die WebView.
    /// </summary>
    private static string BuildLongDescriptionHtml(string contentHtml)
    {
        var content = string.IsNullOrWhiteSpace(contentHtml) ? "<p></p>" : contentHtml;

        return $$"""
        <!doctype html>
        <html lang="de">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0" />
            <style>
                :root { color-scheme: light dark; }
                html, body {
                    background: transparent !important;
                    overflow: hidden;
                }
                body {
                    margin: 0;
                    padding: 0;
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                    font-size: 16px;
                    line-height: 1.5;
                    color: #1b1b1b;
                    background: transparent;
                    word-wrap: break-word;
                }
                a { color: #1953c6; text-decoration: underline; }
                ul, ol { padding-left: 1.2rem; }
                ol li[data-list='bullet'] { list-style-type: disc; }
                ol li[data-list='ordered'] { list-style-type: decimal; }
                ol li[data-list='bullet']::marker { content: '\2022 '; }
                h1, h2, h3 { margin: 0.6rem 0 0.35rem 0; }
                p { margin: 0 0 0.6rem 0; }
                @media (prefers-color-scheme: dark) {
                    body { color: #f2f2f2; }
                    a { color: #7fb0ff; }
                }
            </style>
            <script>
                (function() {
                    function reportHeight() {
                        var root = document.getElementById('content-root');
                        if (!root) return;
                        var h = Math.max(
                            root.scrollHeight,
                            root.offsetHeight,
                            Math.ceil(root.getBoundingClientRect().height)
                        );
                        window.location.href = 'height:' + Math.ceil(h);
                    }

                    document.addEventListener('click', function(ev) {
                        var node = ev.target;
                        if (!node) return;
                        var link = node.closest ? node.closest('a[href]') : null;
                        if (!link) return;

                        var href = link.getAttribute('href');
                        if (!href) return;

                        ev.preventDefault();
                        window.location.href = 'extlink:' + encodeURIComponent(href);
                    }, true);

                    window.addEventListener('load', function() {
                        reportHeight();
                        setTimeout(reportHeight, 120);
                        setTimeout(reportHeight, 450);
                        setTimeout(reportHeight, 1100);
                    });
                    window.addEventListener('resize', reportHeight);
                })();
            </script>
        </head>
        <body>
            <div id="content-root">{{content}}</div>
        </body>
        </html>
        """;
    }

    /// <summary>
    /// Oeffnet die aktuelle Station mit Koordinaten in Komoot.
    /// </summary>
    /// <returns>Asynchroner Vorgang zum Starten der externen App bzw. Website.</returns>
    private async Task OpenInKomootAsync()
    {
        if (Station == null)
            return;

        if (!Station.Latitude.HasValue || !Station.Longitude.HasValue)
        {
            await UiNotify.ToastAsync("Keine Adresse oder Koordinaten hinterlegt.");
            return;
        }

        var lat = Station.Latitude.Value.ToString(CultureInfo.InvariantCulture);
        var lon = Station.Longitude.Value.ToString(CultureInfo.InvariantCulture);
        // Der "Ortsname"-Teil dient als Label und kann aus Station.Title erzeugt werden,
        // alternativ auch aus einem festen Begriff wie "Ort" (URL-encodiert).
        var slug = Uri.EscapeDataString(Station.Title?.Trim() ?? "Ort");

        var sport = "hike";
        var maxDistanceMeters = 9000;

        var uri =
            $"https://www.komoot.com/de-de/discover/{slug}/@{lat},{lon}/tours" +
            $"?sport={sport}&map=true&max_distance={maxDistanceMeters}&pageNumber=1";

        await Launcher.OpenAsync(uri);
    }

    /// <summary>
    /// Oeffnet die Station in einer Karten-App per Koordinaten oder Adresse.
    /// </summary>
    /// <returns>Asynchroner Vorgang zum Starten der Karten-App.</returns>
    private async Task OpenInMapsAsync()
    {
        if (Station == null)
            return;

        string? uri = null;

        if (Station.Latitude.HasValue && Station.Longitude.HasValue)
        {
            var lat = Station.Latitude.Value.ToString(CultureInfo.InvariantCulture);
            var lon = Station.Longitude.Value.ToString(CultureInfo.InvariantCulture);
            uri = $"https://www.google.com/maps/search/?api=1&query={lat},{lon}";
        }
        else
        {
            var parts = new[]
            {
                Station.Title,
                Station.Street,
                Station.HouseNumber,
                Station.ZipCode,
                Station.City
            }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());

            var query = string.Join(" ", parts);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var encoded = Uri.EscapeDataString(query);
                uri = $"https://www.google.com/maps/search/?api=1&query={encoded}";
            }
        }

        if (uri == null)
        {
            await UiNotify.ToastAsync("Keine Adresse oder Koordinaten hinterlegt.");
            return;
        }

        await Launcher.OpenAsync(uri);
    }
}
