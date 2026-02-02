using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind fuer den Stations-Teaser mit Deeplink-Parameterverarbeitung.
/// </summary>
public partial class StationTeaserPage : ContentPage, IQueryAttributable
{
    private readonly StationTeaserViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von StationTeaserPage.
    /// </summary>
    public StationTeaserPage(StationTeaserViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Uebernimmt Navigationsparameter und laedt die dazugehoerigen Inhalte.
    /// </summary>
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("code", out var codeObj) || codeObj is not string code || string.IsNullOrWhiteSpace(code))
        {
            await UiNotify.ToastAsync("Kein gültiger Code übergeben.");
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _viewModel.LoadByCodeAsync(code);

        if (!_viewModel.HasStation)
        {
            await UiNotify.ToastAsync($"Keine Station mit Code '{code}' gefunden.");
            await Shell.Current.GoToAsync("..");
        }
    }
}



