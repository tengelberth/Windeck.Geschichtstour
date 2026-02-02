using Foundation;
using UIKit;

namespace Windeck.Geschichtstour.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
    {
        // Universal Links kommen hier rein
        if (userActivity.ActivityType == NSUserActivityType.BrowsingWeb && userActivity.WebPageUrl != null)
        {
            var nsUrl = userActivity.WebPageUrl;

            // Damit MAUI OnAppLinkRequestReceived(Uri) triggert:
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.OpenUrl(nsUrl);
            });

            return true;
        }

        return base.ContinueUserActivity(application, userActivity, completionHandler);
    }
}
