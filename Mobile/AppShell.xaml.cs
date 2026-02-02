using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile;

/// <summary>
/// Definiert die globale Navigation der App und registriert Routen.
/// </summary>
public partial class AppShell : Shell
{
    public Command OpenWebsiteCommand { get; }

    /// <summary>
    /// Initialisiert Shell-Routen und Kommandos.
    /// </summary>
    /// <param name="appUrlOptions">URL-Konfiguration fuer externe Website-Navigation.</param>
    public AppShell(AppUrlOptions appUrlOptions)
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(StationContentPage), typeof(StationContentPage));
        Routing.RegisterRoute(nameof(StationTeaserPage), typeof(StationTeaserPage));
        Routing.RegisterRoute(nameof(TourTeaserPage), typeof(TourTeaserPage));
        Routing.RegisterRoute(nameof(QrScannerPage), typeof(QrScannerPage));

        OpenWebsiteCommand = new Command(async () =>
            await Launcher.OpenAsync("https://geschichtstour-backend.azurewebsites.net"));

        BindingContext = this;
    }
}
