using System.Globalization;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

public class StationTeaserViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

    private StationDto? _station;
    public StationDto? Station
    {
        get => _station;
        set
        {
            if (SetProperty(ref _station, value))
            {
                OnPropertyChanged(nameof(HasStation));

                // Commands neu bewerten
                (OpenInMapsCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public bool HasStation => Station != null;

    public Command OpenInMapsCommand { get; }
    public Command OpenInKomootCommand { get; }
    public Command ShareStationCommand { get; }

    public StationTeaserViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        OpenInMapsCommand = new Command(async () => await OpenInMapsAsync());
        OpenInKomootCommand = new Command(async () => await OpenInKomootAsync());
        ShareStationCommand = new Command(async () => await ShareStationAsync());
    }

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
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Station (Teaser) für Code {code}: {ex}");
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


    private async Task OpenInKomootAsync()
    {
        if (Station == null)
            return;

        if (!Station.Latitude.HasValue || !Station.Longitude.HasValue)
        {
            await UiNotify.ToastAsync("Keine Koordninaten hinterlegt.");
            return;
        }

        var lat = Station.Latitude.Value.ToString(CultureInfo.InvariantCulture);
        var lon = Station.Longitude.Value.ToString(CultureInfo.InvariantCulture);

        // Der "Ortsname"-Teil ist vor allem ein Label. Du kannst dort z.B. Station.Title verwenden,
        // oder ein fixes Label wie "Ort" / "Selected" etc. (URL-encoden!).
        var slug = Uri.EscapeDataString(Station.Title?.Trim() ?? "Ort");

        var sport = "hike";
        var maxDistanceMeters = 5000;

        var uri =
            $"https://www.komoot.com/de-de/discover/{slug}/@{lat},{lon}/tours" +
            $"?sport={sport}&map=true&max_distance={maxDistanceMeters}&pageNumber=1";

        await Launcher.OpenAsync(uri);
    }

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
