using Foundation;

namespace Windeck.Geschichtstour.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool ContinueUserActivity(UIKit.UIApplication application, NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler)
    {
        if (userActivity.ActivityType == NSUserActivityType.BrowsingWeb && userActivity.WebPageUrl != null)
        {
            if (Uri.TryCreate(userActivity.WebPageUrl.ToString(), UriKind.Absolute, out Uri? uri))
            {
                MainThread.BeginInvokeOnMainThread(() => App.Current?.SendOnAppLinkRequestReceived(uri));
            }

            return true;
        }

        return base.ContinueUserActivity(application, userActivity, completionHandler);
    }
}
