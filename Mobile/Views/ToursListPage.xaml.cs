using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind fuer die Tourenuebersicht der App.
/// </summary>
public partial class ToursListPage : ContentPage
{
    private readonly ToursListViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von ToursListPage.
    /// </summary>
    public ToursListPage(ToursListViewModel viewModel)
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

        if (!_viewModel.Tours.Any())
        {
            await _viewModel.LoadToursAsync();
        }
    }
}


