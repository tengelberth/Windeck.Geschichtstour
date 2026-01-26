using CommunityToolkit.Maui.Views;
using System.Globalization;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

public class StationContentViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

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


    public StationContentViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;

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
    /// Lädt die Station anhand eines Codes von der API.
    /// </summary>
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

    public async Task ShareStationAsync()
    {
        if (Station == null)
            return;

        var url = $"https://geschichtstour-backend.azurewebsites.net/share/station?code={Uri.EscapeDataString(Station.Code)}";

        await Share.RequestAsync(new ShareTextRequest
        {
            Title = Station.Title,
            Text = "Schau dir diese Station an:",
            Uri = url
        });
    }

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

    private void RefreshMediaCommands()
    {
        (NextMediaCommand as Command)?.ChangeCanExecute();
        (PrevMediaCommand as Command)?.ChangeCanExecute();
    }

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

        // Der "Ortsname"-Teil ist vor allem ein Label. Du kannst dort z.B. Station.Title verwenden,
        // oder ein fixes Label wie "Ort" / "Selected" etc. (URL-encoden!).
        var slug = Uri.EscapeDataString(Station.Title?.Trim() ?? "Ort");

        var sport = "hike";
        var maxDistanceMeters = 9000;

        var uri =
            $"https://www.komoot.com/de-de/discover/{slug}/@{lat},{lon}/tours" +
            $"?sport={sport}&map=true&max_distance={maxDistanceMeters}&pageNumber=1";

        await Launcher.OpenAsync(uri);
    }

    /// <summary>
    /// Öffnet die Station in einer Karten-/Navigations-App (z. B. Google Maps).
    /// </summary>
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
