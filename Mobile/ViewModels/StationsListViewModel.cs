using System.Collections.ObjectModel;
using System.Windows.Input;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Views;
using Windeck.Geschichtstour.Mobile.Services;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

public class StationsListViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private StationDto _selectedStation;

    public ObservableCollection<StationDto> Stations { get; } = new();
    public StationDto? SelectedStation
    {
        get => _selectedStation;
        set
        {
            if (SetProperty(ref _selectedStation, value))
            {
                OnStationSelected();
            }
        }
    }

    public Command RefreshCommand { get; }


    public StationsListViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        RefreshCommand = new Command(async () => await LoadStationsAsync());
    }

    public async Task LoadStationsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Stations.Clear();

            var stationsFromApi = await _apiClient.GetStationsAsync();

            foreach (var station in stationsFromApi.OrderBy(s => s.Title))
            {
                Stations.Add(station);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Stationen: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnStationSelected()
    {
        if (SelectedStation == null)
            return;

        await Shell.Current.GoToAsync($"{nameof(StationTeaserPage)}?code={Uri.EscapeDataString(SelectedStation.Code)}");
        SelectedStation = null;
    }
}
