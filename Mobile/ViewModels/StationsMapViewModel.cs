using Mapsui.UI.Maui;
using System.Collections.ObjectModel;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Laedt Stationsdaten fuer die Karte und koordiniert Pin-Auswahl sowie Overlay-Status.
/// </summary>
public class StationsMapViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<StationDto> Stations { get; } = new();
    public ObservableCollection<Pin> Pins { get; } = new();

    private Pin? _selectedPin;
    public Pin? SelectedPin
    {
        get => _selectedPin;
        set
        {
            if (SetProperty(ref _selectedPin, value) && value?.Tag is StationDto station)
                SelectedStation = station;
        }
    }

    private StationDto? _selectedStation;
    public StationDto? SelectedStation
    {
        get => _selectedStation;
        set
        {
            if (SetProperty(ref _selectedStation, value) && _selectedStation != null)
                _ = OnStationSelected();
        }
    }

    private bool _isMapBusy;
    public bool IsMapBusy
    {
        get => _isMapBusy;
        set
        {
            if (SetProperty(ref _isMapBusy, value))
            {
                OnPropertyChanged(nameof(IsOverlayVisible));
                OnPropertyChanged(nameof(BusyText));
            }
        }
    }

    private bool _isViewportInitialized;
    public bool IsViewportInitialized
    {
        get => _isViewportInitialized;
        set
        {
            if (SetProperty(ref _isViewportInitialized, value))
            {
                OnPropertyChanged(nameof(IsOverlayVisible));
                OnPropertyChanged(nameof(BusyText));
            }
        }
    }

    private bool _isPinsSynchronized;
    public bool IsPinsSynchronized
    {
        get => _isPinsSynchronized;
        set
        {
            if (SetProperty(ref _isPinsSynchronized, value))
            {
                OnPropertyChanged(nameof(IsOverlayVisible));
                OnPropertyChanged(nameof(BusyText));
            }
        }
    }

    private bool _hasCompletedInitialStationsLoad;
    public bool HasCompletedInitialStationsLoad
    {
        get => _hasCompletedInitialStationsLoad;
        private set
        {
            if (SetProperty(ref _hasCompletedInitialStationsLoad, value))
            {
                OnPropertyChanged(nameof(IsOverlayVisible));
                OnPropertyChanged(nameof(BusyText));
            }
        }
    }

    public bool IsOverlayVisible =>
        !IsViewportInitialized ||
        IsBusy ||
        (HasCompletedInitialStationsLoad && !IsPinsSynchronized);

    public string BusyText =>
        !IsViewportInitialized ? "Karte wird initialisiert..." :
        IsBusy ? "Stationen werden geladen..." :
        (HasCompletedInitialStationsLoad && !IsPinsSynchronized) ? "Pins werden gesetzt..." :
        IsMapBusy ? "Karte laedt..." :
        string.Empty;

    /// <summary>
    /// Initialisiert eine neue Instanz von StationsMapViewModel.
    /// </summary>
    public StationsMapViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Laedt Stationsdaten aus dem Backend und aktualisiert den ViewModel-Zustand.
    /// </summary>
    public async Task LoadStationsAsync()
    {
        if (IsBusy) return;

        try
        {
            HasCompletedInitialStationsLoad = false;
            IsBusy = true;
            OnPropertyChanged(nameof(IsOverlayVisible));
            OnPropertyChanged(nameof(BusyText));

            Stations.Clear();
            Pins.Clear();

            var stations = await _apiClient.GetStationsAsync();

            foreach (var s in stations.Where(x => x.Latitude.HasValue && x.Longitude.HasValue))
            {
                Stations.Add(s);

                Pins.Add(new Pin
                {
                    Label = s.Title,
                    Address = $"{s.Street} {s.HouseNumber}, {s.ZipCode} {s.City}",
                    Position = new Position(s.Latitude!.Value, s.Longitude!.Value),
                    Tag = s,
                    Color = Color.FromArgb("#1953c6"),
                    Scale = 0.8f,
                });
            }
        }
        finally
        {
            IsBusy = false;
            HasCompletedInitialStationsLoad = true;
            OnPropertyChanged(nameof(IsOverlayVisible));
            OnPropertyChanged(nameof(BusyText));
            System.Diagnostics.Debug.WriteLine($"Pins: {Pins.Count}");
        }
    }

    /// <summary>
    /// Reagiert auf die Stationsauswahl und navigiert zur Detailansicht.
    /// </summary>
    private async Task OnStationSelected()
    {
        if (SelectedStation == null) return;

        var code = SelectedStation.Code;

        // wichtig: Selection resetten, damit derselbe Pin danach wieder klickbar ist
        SelectedPin = null;
        SelectedStation = null;

        await Shell.Current.GoToAsync(
            $"{nameof(Views.StationTeaserPage)}?code={Uri.EscapeDataString(code)}");
    }
}
