using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

public partial class StationTeaserPage : ContentPage, IQueryAttributable
{
    private readonly StationTeaserViewModel _viewModel;

    public StationTeaserPage(StationTeaserViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

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

