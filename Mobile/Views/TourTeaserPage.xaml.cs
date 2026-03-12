using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind für den Tour-Teaser mit Deeplink-Parameterverarbeitung.
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
    /// Übernimmt Navigationsparameter und lädt die dazugehörigen Inhalte.
    /// </summary>
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tour", out object? tourObj) && tourObj is TourDto tour)
        {
            _viewModel.Tour = tour;
            return;
        }

        if (!TryReadTourId(query, out int id))
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

    private static bool TryReadTourId(IDictionary<string, object> query, out int id)
    {
        id = 0;

        if (!query.TryGetValue("id", out object? idObj) || idObj is null)
        {
            return false;
        }

        return idObj switch
        {
            int intId => (id = intId) > 0,
            string idStr => int.TryParse(idStr, out id) && id > 0,
            _ => int.TryParse(idObj.ToString(), out id) && id > 0
        };
    }
}
