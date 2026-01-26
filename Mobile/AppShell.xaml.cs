using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Route für die Content-Seite, damit wir per Code navigieren können
        Routing.RegisterRoute(nameof(StationContentPage), typeof(StationContentPage));
        Routing.RegisterRoute(nameof(StationTeaserPage), typeof(StationTeaserPage));
        Routing.RegisterRoute(nameof(TourTeaserPage), typeof(TourTeaserPage));
        Routing.RegisterRoute(nameof(QrScannerPage), typeof(QrScannerPage));

    }
}
