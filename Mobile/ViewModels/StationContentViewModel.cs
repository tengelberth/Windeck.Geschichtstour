using CommunityToolkit.Maui.Views;
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

    // stabile Liste für CarouselView + Count + Index
    private List<MediaItemDto> _imageMediaItems = new();
    public IReadOnlyList<MediaItemDto> ImageMediaItems => _imageMediaItems;

    public bool HasImages => _imageMediaItems.Count > 0;

    public StationDto? Station
    {
        get => _station;
        set
        {
            if (SetProperty(ref _station, value))
            {
                // Abhängige Properties benachrichtigen
                OnPropertyChanged(nameof(HasStation));

                // Zusätzliche Logik nach der Änderung
                RebuildImageMediaItems();

                // Commands neu bewerten
                (OpenInMapsCommand as Command)?.ChangeCanExecute();
            }
        }
    }


    public bool HasStation => Station != null;

    // Carousel Position
    private int _mediaPosition;
    public int MediaPosition
    {
        get => _mediaPosition;
        set
        {
            if (SetProperty(ref _mediaPosition, value))
            {
                // Zusätzliche Logik nach der Änderung
                RefreshMediaCommands();
            }
        }
    }

    // Pfeil-Commands
    public Command NextMediaCommand { get; }
    public Command PrevMediaCommand { get; }

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

        NextMediaCommand = new Command(
            execute: () => MediaPosition = Math.Min(MediaPosition + 1, _imageMediaItems.Count - 1),
            canExecute: () => _imageMediaItems.Count > 0 && MediaPosition < _imageMediaItems.Count - 1
        );

        PrevMediaCommand = new Command(
            execute: () => MediaPosition = Math.Max(MediaPosition - 1, 0),
            canExecute: () => _imageMediaItems.Count > 0 && MediaPosition > 0
        );

        OpenInMapsCommand = new Command(async () => await OpenInMapsAsync(), () => Station != null);
        OpenInKomootCommand = new Command(async () => await OpenInKomootAsync());
        ShareStationCommand = new Command(async () => await ShareStationAsync());

        OpenMediaOverlayCommand = new Command<MediaItemDto>(async (item) =>
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

        try
        {
            IsBusy = true;
            Station = null;

            var station = await _apiClient.GetStationByCodeAsync(code);
            Station = station;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Station mit Code {code}: {ex}");
            Station = null;
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

        var url = new Uri(_appUrlOptions.PublicBaseUri, $"share/station?code={Uri.EscapeDataString(Station.Code)}").ToString();

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

        // Position sauber setzen (z.B. nach neuem Laden)
        MediaPosition = _imageMediaItems.Count > 0 ? Math.Clamp(MediaPosition, 0, _imageMediaItems.Count - 1) : 0;

        RefreshMediaCommands();
    }

    /// <summary>
    /// Aktualisiert die CanExecute-Zustaende der Mediennavigation.
    /// </summary>
    private void RefreshMediaCommands()
    {
        (NextMediaCommand as Command)?.ChangeCanExecute();
        (PrevMediaCommand as Command)?.ChangeCanExecute();
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
            await UiNotify.ToastAsync("Keine Adresse oder Koordinaten  hinterlegt.");
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
            await UiNotify.ToastAsync("Keine Adresse oder Koordinaten  hinterlegt.");
            return;
        }

        await Launcher.OpenAsync(uri);
    }
}






