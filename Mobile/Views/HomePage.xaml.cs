using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind der Startseite fuer Codesuche und Hauptnavigation.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    /// <summary>
    /// Initialisiert eine neue Instanz von HomePage.
    /// </summary>
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}

