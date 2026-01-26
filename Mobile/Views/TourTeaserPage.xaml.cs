using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

public partial class TourTeaserPage : ContentPage, IQueryAttributable
{
    private readonly TourTeaserViewModel _viewModel;

    public TourTeaserPage(TourTeaserViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

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
