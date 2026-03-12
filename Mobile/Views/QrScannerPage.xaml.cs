using Windeck.Geschichtstour.Mobile.ViewModels;
using ZXing.Net.Maui;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind fuer die QR-Scanner-Seite mit Bindung an das Scanner-ViewModel.
/// </summary>
public partial class QrScannerPage : ContentPage
{
    private readonly QrScannerViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von QrScannerPage.
    /// </summary>
    public QrScannerPage(QrScannerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        CameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.TwoDimensional,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.AppearingCommand.CanExecute(null))
        {
            _viewModel.AppearingCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        if (_viewModel.DisappearingCommand.CanExecute(null))
        {
            _viewModel.DisappearingCommand.Execute(null);
        }

        base.OnDisappearing();
    }

    private void CameraView_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        string? raw = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel.BarcodeDetectedCommand.CanExecute(raw))
            {
                _viewModel.BarcodeDetectedCommand.Execute(raw);
            }
        });
    }
}
