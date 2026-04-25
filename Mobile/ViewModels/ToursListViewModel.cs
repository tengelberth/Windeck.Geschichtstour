using System.Collections.ObjectModel;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Lädt und verwaltet die Tourenübersicht für die Listenansicht.
/// </summary>
public class ToursListViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private TourDto? _selectedTour;

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
    /// Lädt Tourdaten aus dem Backend und aktualisiert den ViewModel-Zustand.
    /// </summary>
    public async Task LoadToursAsync()
    {
        if (IsBusy)
        {
            return;
        }

        bool hasExistingData = Tours.Count > 0;
        bool loadedFromCache = false;
        bool showLoadingFeedback = hasExistingData;

        try
        {
            if (!hasExistingData)
            {
                List<TourDto>? cachedTours = await _apiClient.TryGetCachedToursAsync();
                if (cachedTours is { Count: > 0 })
                {
                    ApplyTours(cachedTours);
                    loadedFromCache = true;
                }
            }

            showLoadingFeedback = hasExistingData || !loadedFromCache;
            if (showLoadingFeedback)
            {
                IsBusy = true;
                StartLoadingFeedback(
                    hasExistingData
                        ? "Ich hole kurz die neuesten Touren rein."
                        : "Hoppla, da hast du uns wohl beim Nickerchen erwischt.",
                    hasExistingData
                    ? "Ich gleiche kurz ab, ob sich etwas verändert hat."
                        : "Wir wecken kurz den Server auf.",
                    hasExistingData
                        ? "Fast da - die Tourenliste wird gerade aufgefrischt."
                    : "Im Hintergrund fährt jetzt auch die Datenbank hoch.",
                    hasExistingData
                        ? "Gleich fertig - ich sortiere nur noch alles ein."
                        : "Das dauert einen kleinen Moment. So sparen wir jedoch laufende Kosten.",
                    hasExistingData
                        ? "Fertig - die Touren sind gleich wieder aktuell."
                    : "Sobald der Server wach ist, werden die nächsten Anfragen deutlich schneller bearbeitet.",
                    "Fast da - wir sammeln die Inhalte jetzt für dich zusammen.",
                    "Dir gefällt die App oder du hast Verbesserungsvorschläge? Dann schreib uns gern eine Rezension im Store.",
                    "Du findest, es fehlen noch Stationen? Dann melde dich und gestalte die Inhalte mit.");
            }

            List<TourDto> toursFromApi = await _apiClient.GetToursAsync(
                allowUserRetryUi: false,
                cancellationToken: default);

            if (toursFromApi.Count > 0 || !loadedFromCache)
            {
                ApplyTours(toursFromApi);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Touren: {ex}");
        }
        finally
        {
            if (showLoadingFeedback)
            {
                StopLoadingFeedback();
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// Übernimmt eine geladene Tourmenge sortiert in die ObservableCollection.
    /// </summary>
    private void ApplyTours(IEnumerable<TourDto> tours)
    {
        Tours.Clear();
        foreach (TourDto tour in tours.OrderBy(t => t.Title))
        {
            Tours.Add(tour);
        }
    }

    /// <summary>
    /// Reagiert auf die Tourauswahl und navigiert zur Detailansicht.
    /// </summary>
    private async void OnTourSelected()
    {
        if (SelectedTour == null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(TourTeaserPage), new Dictionary<string, object>
        {
            ["tour"] = SelectedTour
        });
        SelectedTour = null;
    }
}
