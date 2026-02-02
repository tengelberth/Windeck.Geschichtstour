using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Windeck.Geschichtstour.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

    [IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "geschichtstour-backend.azurewebsites.net",
    DataPathPrefix = "/",
    AutoVerify = true)]
    [IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "geschichtstour.windecker-laendchen.com",
    DataPathPrefix = "/",
    AutoVerify = true)]
    [IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "geschichtstour.gemeinde-windeck.de",
    DataPathPrefix = "/",
    AutoVerify = true)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
