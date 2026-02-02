using Windeck.Geschichtstour.Mobile.Helper;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Steuert den QR-Scanner-Workflow und die Weiterleitung zur passenden Station.
/// </summary>
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

    /// <summary>
    /// Initialisiert eine neue Instanz von QrScannerViewModel.
    /// </summary>
    public QrScannerViewModel()
    {
        AppearingCommand = new Command(async () => await OnAppearingAsync());
        DisappearingCommand = new Command(OnDisappearing);
        BarcodeDetectedCommand = new Command<string>(async raw => await OnBarcodeDetectedAsync(raw));
    }

    /// <summary>
    /// Wird beim Anzeigen aufgerufen und laedt asynchron die benoetigten Daten.
    /// </summary>
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

    /// <summary>
    /// Wird beim Verlassen der Seite aufgerufen und beendet laufende Aktionen.
    /// </summary>
    private void OnDisappearing()
    {
        IsDetecting = false;
    }

    /// <summary>
    /// Verarbeitet einen erkannten Barcode und startet die Folgelogik.
    /// </summary>
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


