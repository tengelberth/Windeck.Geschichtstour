using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Container-Seite fuer Stationsliste und Kartenansicht.
/// </summary>
public partial class StationsPage : ContentPage
{
    private readonly StationsListViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von StationsPage.
    /// </summary>
    public StationsPage(StationsListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}


