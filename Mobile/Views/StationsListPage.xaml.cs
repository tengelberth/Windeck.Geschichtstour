using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

public partial class StationsListPage : ContentPage
{
    private readonly StationsListViewModel _viewModel;

    public StationsListPage(StationsListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.Stations.Any())
        {
            await _viewModel.LoadStationsAsync();
        }
    }
}
