using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

public partial class ToursListPage : ContentPage
{
    private readonly ToursListViewModel _viewModel;

    public ToursListPage(ToursListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.Tours.Any())
        {
            await _viewModel.LoadToursAsync();
        }
    }
}
