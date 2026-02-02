using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind fuer die Listenansicht aller Stationen.
/// </summary>
public partial class StationsListPage : ContentPage
{
    private readonly StationsListViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von StationsListPage.
    /// </summary>
    public StationsListPage(StationsListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Wird beim Anzeigen der Seite aufgerufen und startet Initialisierungslogik.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.Stations.Any())
        {
            await _viewModel.LoadStationsAsync();
        }
    }
}


