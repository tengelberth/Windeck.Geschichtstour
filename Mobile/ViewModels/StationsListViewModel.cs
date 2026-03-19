using System.Collections.ObjectModel;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Lädt und verwaltet die Stationsliste fuer die Listenansicht.
/// </summary>
public class StationsListViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private StationDto? _selectedStation;

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

    /// <summary>
    /// Initialisiert eine neue Instanz von StationsListViewModel.
    /// </summary>
    public StationsListViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        RefreshCommand = new Command(async () => await LoadStationsAsync());
    }

    /// <summary>
    /// Lädt Stationsdaten aus dem Backend und aktualisiert den ViewModel-Zustand.
    /// </summary>
    public async Task LoadStationsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        bool hasExistingData = Stations.Count > 0;
        bool loadedFromCache = false;
        bool showLoadingFeedback = hasExistingData;

        try
        {
            if (!hasExistingData)
            {
                List<StationDto>? cachedStations = await _apiClient.TryGetCachedStationsAsync();
                if (cachedStations is { Count: > 0 })
                {
                    ApplyStations(cachedStations);
                    loadedFromCache = true;
                }
            }

            showLoadingFeedback = hasExistingData || !loadedFromCache;
            if (showLoadingFeedback)
            {
                IsBusy = true;
                StartLoadingFeedback(
                    hasExistingData
                        ? "Ich hole kurz die neuesten Stationen rein."
                        : "Hoppla, da hast du uns wohl beim Nickerchen erwischt.",
                    hasExistingData
                    ? "Ich gleiche gerade ab, ob sich etwas verändert hat."
                        : "Wir wecken gerade kurz den Server auf.",
                    hasExistingData
                        ? "Fast da - die Liste wird gerade aufgefrischt."
                    : "Im Hintergrund fährt jetzt auch die Datenbank hoch.",
                    hasExistingData
                        ? "Gleich fertig - ich sortiere nur noch alles ein."
                        : "Das dauert einen kleinen Moment. So sparen wir jedoch laufende Kosten.",
                    hasExistingData
                        ? "Fertig - die Stationen sind gleich wieder aktuell."
                    : "Sobald der Server wach ist, werden die nächsten Anfragen deutlich schneller bearbeitet.",

                    "Fast da - wir sammeln die Inhalte gerade für dich zusammen.",
                    "Dir gefällt die App oder du hast Verbesserungsvorschläge? Dann schreib uns gern eine Rezension im Store.",
                    "Du findest, es fehlen noch Stationen? Dann melde dich und gestalte die Inhalte mit.");
            }

            List<StationDto> stationsFromApi = await _apiClient.GetStationsAsync(
                allowUserRetryUi: false,
                cancellationToken: default);

            if (stationsFromApi.Count > 0 || !loadedFromCache)
            {
                ApplyStations(stationsFromApi);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Stationen: {ex}");
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
    /// Übernimmt eine geladene Stationsmenge sortiert in die ObservableCollection.
    /// </summary>
    private void ApplyStations(IEnumerable<StationDto> stations)
    {
        Stations.Clear();
        foreach (StationDto station in stations.OrderBy(s => s.Title))
        {
            Stations.Add(station);
        }
    }

    /// <summary>
    /// Reagiert auf die Stationsauswahl und navigiert direkt zur Inhaltsseite.
    /// </summary>
    private async void OnStationSelected()
    {
        if (SelectedStation == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(StationContentPage)}?code={Uri.EscapeDataString(SelectedStation.Code)}");
        SelectedStation = null;
    }
}

