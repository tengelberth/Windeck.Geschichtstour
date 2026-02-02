using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind fuer den Tour-Teaser mit Deeplink-Parameterverarbeitung.
/// </summary>
public partial class TourTeaserPage : ContentPage, IQueryAttributable
{
    private readonly TourTeaserViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von TourTeaserPage.
    /// </summary>
    public TourTeaserPage(TourTeaserViewModel viewModel)
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
        if (!query.TryGetValue("id", out var idObj) || idObj is not string idStr || !int.TryParse(idStr, out var id))
        {
            await UiNotify.ToastAsync("Keine gültige Tour-ID übergeben.");
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _viewModel.LoadByIdAsync(id);

        if (!_viewModel.HasTour)
        {
            await UiNotify.ToastAsync($"Keine Tour mit ID '{id}' gefunden.");
            await Shell.Current.GoToAsync("..");
        }
    }
}


