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
}


