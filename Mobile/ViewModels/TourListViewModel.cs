using System.Collections.ObjectModel;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Laedt und verwaltet die Tourenuebersicht fuer die Listenansicht.
/// </summary>
public class ToursListViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private TourDto _selectedTour;

    public ObservableCollection<TourDto> Tours { get; } = new();
    public TourDto? SelectedTour
    {
        get => _selectedTour;
        set
        {
            if (SetProperty(ref _selectedTour, value))
            {
                OnTourSelected();
            }
        }
    }

    public Command RefreshCommand { get; }


    /// <summary>
    /// Initialisiert eine neue Instanz von ToursListViewModel.
    /// </summary>
    public ToursListViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        RefreshCommand = new Command(async () => await LoadToursAsync());
    }

    /// <summary>
    /// Laedt Tourdaten aus dem Backend und aktualisiert den ViewModel-Zustand.
    /// </summary>
    public async Task LoadToursAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Tours.Clear();

            var tours = await _apiClient.GetToursAsync();

            foreach (var tour in tours.OrderBy(t => t.Title))
            {
                Tours.Add(tour);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Touren: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Reagiert auf die Tourauswahl und navigiert zur Detailansicht.
    /// </summary>
    private async void OnTourSelected()
    {
        if (SelectedTour == null)
            return;

        await Shell.Current.GoToAsync($"{nameof(TourTeaserPage)}?id={SelectedTour.Id}");
        SelectedTour = null;
    }
}


