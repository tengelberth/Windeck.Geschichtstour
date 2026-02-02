using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.ViewModels;
using Windeck.Geschichtstour.Mobile.Views;
using ZXing.Net.Maui.Controls;


namespace Windeck.Geschichtstour.Mobile
{
    /// <summary>
    /// Konfiguriert den App-Start, den DI-Container und den Plattform-Lifecycle der MAUI-Anwendung.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Konfiguriert und erstellt die MAUI-Anwendung inklusive Services und Navigation.
        /// </summary>
        /// <returns>Vollstaendig konfigurierte Instanz der MAUI-Anwendung.</returns>
        public static MauiApp CreateMauiApp()
        {
            AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.ConfigureLifecycleEvents(lifecycle =>
            {
#if ANDROID
                lifecycle.AddAndroid(android =>
                {
                    android.OnCreate((activity, bundle) =>
                    {
                        TryHandleLink(activity.Intent);
                    });

                    android.OnNewIntent((activity, intent) =>
                    {
                        TryHandleLink(intent);
                    });
                });
#endif

#if IOS || MACCATALYST
                lifecycle.AddiOS(ios =>
                {
                    ios.FinishedLaunching((app, data) => HandleAppLink(app.UserActivity));
                    ios.ContinueUserActivity((app, userActivity, handler) => HandleAppLink(userActivity));
                });
#endif
            });

#if ANDROID
            static void TryHandleLink(Android.Content.Intent? intent)
            {
                var action = intent?.Action;
                var data = intent?.Data?.ToString();

                if (action == Android.Content.Intent.ActionView && data is not null
                    && Uri.TryCreate(data, UriKind.Absolute, out var uri))
                {
                    App.Current?.SendOnAppLinkRequestReceived(uri);
                }
            }
#endif

#if IOS || MACCATALYST
            static bool HandleAppLink(Foundation.NSUserActivity? userActivity)
            {
                if (userActivity is not null &&
                    userActivity.ActivityType == Foundation.NSUserActivityType.BrowsingWeb &&
                    userActivity.WebPageUrl is not null)
                {
                    if (Uri.TryCreate(userActivity.WebPageUrl.ToString(), UriKind.Absolute, out var uri))
                        App.Current?.SendOnAppLinkRequestReceived(uri);

                    return true;
                }
                return false;
            }
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var appUrlOptions = AppUrlOptionsLoader.Load();
            builder.Services.AddSingleton(appUrlOptions);

            // Konfigurieren der DI-Container und der App
            builder.Services.AddSingleton<AppShell>();

            // Registriere den ApiClient als Singleton, damit immer nur eine Instanz existiert
            builder.Services.AddSingleton<ApiClient>();

            // Registriere ViewModels als Transient, weil sie für jede Seite/Anforderung neu erstellt werden
            builder.Services.AddTransient<StationContentViewModel>();
            builder.Services.AddTransient<StationTeaserViewModel>();
            builder.Services.AddTransient<TourTeaserViewModel>();
            builder.Services.AddTransient<QrScannerViewModel>();

            // ViewModels, die die gesamte Lebensdauer der App verwenden, als Singleton registrieren
            builder.Services.AddSingleton<StationsListViewModel>();
            builder.Services.AddSingleton<StationsMapViewModel>();
            builder.Services.AddSingleton<ToursListViewModel>();
            builder.Services.AddSingleton<HomeViewModel>();

            // Registriere Seiten, die ViewModels benötigen
            builder.Services.AddTransient<StationContentPage>();
            builder.Services.AddTransient<StationTeaserPage>();
            builder.Services.AddTransient<TourTeaserPage>();
            builder.Services.AddTransient<QrScannerPage>();

            // Seiten, die nur einmal existieren und für die Lebensdauer der App genutzt werden, als Singleton registrieren
            builder.Services.AddSingleton<StationsPage>();
            builder.Services.AddSingleton<StationsListPage>();
            builder.Services.AddSingleton<StationsMapPage>();
            builder.Services.AddSingleton<ToursListPage>();
            builder.Services.AddSingleton<AboutPage>();
            builder.Services.AddSingleton<HomePage>();

            // Konfiguriere die App
            builder.Services.AddSingleton<App>();

            return builder.Build();
        }
    }
}

