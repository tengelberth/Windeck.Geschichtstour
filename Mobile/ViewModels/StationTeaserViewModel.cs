using System.Globalization;
using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Laedt Stations-Teaserdaten und stellt Aktionen fuer Teilen und Navigation bereit.
/// </summary>
public class StationTeaserViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly AppUrlOptions _appUrlOptions;

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

    /// <summary>
    /// Initialisiert das ViewModel mit API-Zugriff und URL-Konfiguration.
    /// </summary>
    /// <param name="apiClient">API-Service zum Laden der Stationsdaten.</param>
    /// <param name="appUrlOptions">URL-Konfiguration fuer Share-Links.</param>
    public StationTeaserViewModel(ApiClient apiClient, AppUrlOptions appUrlOptions)
    {
        _apiClient = apiClient;
        _appUrlOptions = appUrlOptions;
        OpenInMapsCommand = new Command(async () => await OpenInMapsAsync());
        OpenInKomootCommand = new Command(async () => await OpenInKomootAsync());
        ShareStationCommand = new Command(async () => await ShareStationAsync());
    }

    /// <summary>
    /// Laedt die Station fuer den uebergebenen Code und setzt den Anzeigestatus.
    /// </summary>
    /// <param name="code">Stationscode aus QR-Code oder Deeplink.</param>
    /// <returns>Asynchroner Vorgang zum Laden und Setzen der Station.</returns>
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
    /// Oeffnet die aktuelle Station mit Koordinaten in Komoot.
    /// </summary>
    /// <returns>Asynchroner Vorgang zum Starten der externen App bzw. Website.</returns>
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

        // Der "Ortsname"-Teil dient als Label und kann aus Station.Title erzeugt werden,
        // alternativ auch aus einem festen Begriff wie "Ort" (URL-encodiert).
        var slug = Uri.EscapeDataString(Station.Title?.Trim() ?? "Ort");

        var sport = "hike";
        var maxDistanceMeters = 5000;

        var uri =
            $"https://www.komoot.com/de-de/discover/{slug}/@{lat},{lon}/tours" +
            $"?sport={sport}&map=true&max_distance={maxDistanceMeters}&pageNumber=1";

        await Launcher.OpenAsync(uri);
    }

    /// <summary>
    /// Oeffnet die aktuelle Station in einer Karten-App per Koordinaten oder Adresse.
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







