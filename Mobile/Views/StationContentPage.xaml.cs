using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Content-Seite einer Station.
/// Wird ausschließlich über einen Code (z. B. QR-Code) geöffnet.
/// </summary>
public partial class StationContentPage : ContentPage, IQueryAttributable
{
    private readonly StationContentViewModel _viewModel;

    public StationContentPage(StationContentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Wird von Shell aufgerufen, wenn die Seite über GoToAsync mit Query-Param geöffnet wird
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
