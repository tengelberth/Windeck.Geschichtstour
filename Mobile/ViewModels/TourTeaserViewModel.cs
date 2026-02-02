using System.Globalization;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

public class TourTeaserViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

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
                // Wenn sich _tour geändert hat, werden auch die abhängigen Properties aktualisiert
                OnPropertyChanged(nameof(HasTour));
                OnPropertyChanged(nameof(OrderedStops)); // Wenn OrderedStops von Tour abhängt
            }
        }
    }

    public bool HasTour => Tour != null;

    public Command StartTourNavigationCommand { get; }

    public TourTeaserViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        StartTourNavigationCommand = new Command(async () => await OpenTourInMapsAsync());
    }

    public async Task LoadByIdAsync(int id)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Tour = null;

            var tour = await _apiClient.GetTourByIdAsync(id);
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

    private async Task OpenTourInMapsAsync()
    {
        if (Tour == null)
            return;

        var stopsWithCoords = Tour.Stops
            .Where(s => s != null && s.Latitude.HasValue && s.Longitude.HasValue)
            .OrderBy(s => s.Order)
            .ToList();

        if (stopsWithCoords.Count < 2)
        {
            await UiNotify.ToastAsync("Keine Tourdaten zum Berechnen verfügbar.");
            return;
        }

        var destination = stopsWithCoords.Last()!;
        var waypoints = stopsWithCoords.SkipLast(1).ToList(); // Waypoints ohne das Ziel

        string FormatCoords(TourStopDto t) =>
            $"{t.Latitude!.Value.ToString(CultureInfo.InvariantCulture)},{t.Longitude!.Value.ToString(CultureInfo.InvariantCulture)}";

        var destStr = FormatCoords(destination);

        string url = $"https://www.google.com/maps/dir/?api=1&destination={destStr}";

        if (waypoints.Any())
        {
            var wp = string.Join("|", waypoints.Select(FormatCoords));
            url += $"&waypoints={wp}";
        }

        await Launcher.OpenAsync(url);
    }
}
