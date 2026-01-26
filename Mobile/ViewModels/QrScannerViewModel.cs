using Windeck.Geschichtstour.Mobile.Helper;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

public class QrScannerViewModel : BaseViewModel
{
    private bool _isDetecting;
    private int _handling; // 0/1 gegen Mehrfach-Trigger

    public bool IsDetecting
    {
        get => _isDetecting;
        set => SetProperty(ref _isDetecting, value);
    }

    public Command AppearingCommand { get; }
    public Command DisappearingCommand { get; }
    public Command<string> BarcodeDetectedCommand { get; }

    public QrScannerViewModel()
    {
        AppearingCommand = new Command(async () => await OnAppearingAsync());
        DisappearingCommand = new Command(OnDisappearing);
        BarcodeDetectedCommand = new Command<string>(async raw => await OnBarcodeDetectedAsync(raw));
    }

    private async Task OnAppearingAsync()
    {
        Interlocked.Exchange(ref _handling, 0);

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await UiNotify.SnackbarAsync(
    "Kamera-Berechtigung fehlt.",
    "Einstellungen",
    () => AppInfo.ShowSettingsUI()
);
            await Shell.Current.GoToAsync("..");
            return;
        }

        IsDetecting = true;
    }

    private void OnDisappearing()
    {
        IsDetecting = false;
    }

    private async Task OnBarcodeDetectedAsync(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return;

        if (Interlocked.Exchange(ref _handling, 1) == 1)
            return;

        // Parsing darf im Background passieren
        var code = QrCodeParser.TryExtractCode(raw);

        // Alles UI-relevante auf den MainThread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            IsDetecting = false;

            if (string.IsNullOrWhiteSpace(code))
            {
                await UiNotify.ToastAsync("QR-Code ungültig. Bitte nutze die Texteingabe.");

                Interlocked.Exchange(ref _handling, 0);
                IsDetecting = true;
                return;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(StationContentPage)}?code={Uri.EscapeDataString(code)}");
        });
    }
}
