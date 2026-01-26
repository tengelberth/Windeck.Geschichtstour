using Windeck.Geschichtstour.Mobile.ViewModels;
using ZXing.Net.Maui;

namespace Windeck.Geschichtstour.Mobile.Views;

public partial class QrScannerPage : ContentPage
{
    private readonly QrScannerViewModel _viewModel;
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
}