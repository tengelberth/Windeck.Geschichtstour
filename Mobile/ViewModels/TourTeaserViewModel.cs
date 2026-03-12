using System.Globalization;
using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Lädt und präsentiert Detailinformationen zu einer ausgewählten Tour.
/// </summary>
public class TourTeaserViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly AppUrlOptions _appUrlOptions;

    private TourDto? _tour;

    public IEnumerable<TourStopDto> OrderedStops =>
        Tour?.Stops?
            .OrderBy(s => s.Order)
        ?? Enumerable.Empty<TourStopDto>();

    public TourDto? Tour
    {
        get => _tour;
        set
        {
            if (SetProperty(ref _tour, value))
            {
                OnPropertyChanged(nameof(HasTour));
                OnPropertyChanged(nameof(OrderedStops));
                OnPropertyChanged(nameof(StopCountText));
            }
        }
    }

    public bool HasTour => Tour != null;
    public string StopCountText => Tour == null ? string.Empty : $"{Tour.Stops.Count} Stationen";

    public Command StartTourNavigationCommand { get; }
    public Command<TourStopDto> OpenTourStopCommand { get; }
    public Command ShareTourCommand { get; }

    /// <summary>
    /// Initialisiert eine neue Instanz von TourTeaserViewModel.
    /// </summary>
    public TourTeaserViewModel(ApiClient apiClient, AppUrlOptions appUrlOptions)
    {
        _apiClient = apiClient;
        _appUrlOptions = appUrlOptions;
        StartTourNavigationCommand = new Command(async () => await OpenTourInMapsAsync());
        OpenTourStopCommand = new Command<TourStopDto>(async stop => await OpenTourStopAsync(stop));
        ShareTourCommand = new Command(async () => await ShareTourAsync());
    }

    /// <summary>
    /// Lädt einen Datensatz anhand der übergebenen ID.
    /// </summary>
    public async Task LoadByIdAsync(int id)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Tour = null;

            TourDto? tour = await _apiClient.GetTourByIdAsync(id);
            Tour = tour;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Tour {id}: {ex}");
            Tour = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Öffnet die ausgewählte Tour in der Karten-App.
    /// </summary>
    private async Task OpenTourInMapsAsync()
    {
        if (Tour == null)
        {
            return;
        }

        List<TourStopDto> stopsWithCoords = Tour.Stops
            .Where(s => s != null && s.Latitude.HasValue && s.Longitude.HasValue)
            .OrderBy(s => s.Order)
            .ToList();

        if (stopsWithCoords.Count < 2)
        {
            await UiNotify.ToastAsync("Keine Tourdaten zum Berechnen verfügbar.");
            return;
        }

        TourStopDto destination = stopsWithCoords.Last()!;
        List<TourStopDto> waypoints = stopsWithCoords.SkipLast(1).ToList();

        string FormatCoords(TourStopDto t) =>
            $"{t.Latitude!.Value.ToString(CultureInfo.InvariantCulture)},{t.Longitude!.Value.ToString(CultureInfo.InvariantCulture)}";

        string destStr = FormatCoords(destination);
        string url = $"https://www.google.com/maps/dir/?api=1&destination={destStr}";

        if (waypoints.Any())
        {
            string wp = string.Join("|", waypoints.Select(FormatCoords));
            url += $"&waypoints={wp}";
        }

        await Launcher.OpenAsync(url);
    }

    /// <summary>
    /// Öffnet den Inhalt einer Station direkt aus der Tourliste.
    /// </summary>
    private async Task OpenTourStopAsync(TourStopDto? stop)
    {
        if (string.IsNullOrWhiteSpace(stop?.StationCode))
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(StationContentPage)}?code={Uri.EscapeDataString(stop.StationCode)}");
    }

    /// <summary>
    /// Erstellt einen öffentlichen Share-Link zur aktuellen Tour.
    /// </summary>
    private async Task ShareTourAsync()
    {
        if (Tour == null)
        {
            return;
        }

        string url = new Uri(_appUrlOptions.PublicBaseUri, $"tour?id={Tour.Id}").ToString();

        await Share.RequestAsync(new ShareTextRequest
        {
            Title = Tour.Title,
            Text = "Schau dir diese Tour an:",
            Uri = url
        });
    }
}
