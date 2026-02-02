using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile
{
    /// <summary>
    /// Einstiegspunkt der MAUI-Anwendung und zentrale Deeplink-Verarbeitung.
    /// </summary>
    public partial class App : Application
    {
        private readonly AppUrlOptions _appUrlOptions;

        /// <summary>
        /// Initialisiert die Anwendung und setzt die Shell als Hauptseite.
        /// </summary>
        /// <param name="serviceProvider">DI-Container zum Aufloesen von AppShell.</param>
        /// <param name="appUrlOptions">Konfiguration fuer gueltige Deeplink-Hosts.</param>
        public App(IServiceProvider serviceProvider, AppUrlOptions appUrlOptions)
        {
            _appUrlOptions = appUrlOptions;
            InitializeComponent();

            MainPage = serviceProvider.GetRequiredService<AppShell>();
        }

        /// <summary>
        /// Verarbeitet eingehende Deeplinks und navigiert zur passenden Seite.
        /// </summary>
        /// <param name="uri">Eingehender Deeplink.</param>
        /// <remarks>Akzeptiert nur Hosts aus der konfigurierten Allowlist.</remarks>
        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            await Dispatcher.DispatchAsync(async () =>
            {
                if (!_appUrlOptions.AllowedDeepLinkHosts.Contains(uri.Host))
                    return;

                var code = GetQueryParam(uri, "code");
                if (string.IsNullOrWhiteSpace(code))
                    return;

                var path = uri.AbsolutePath.TrimEnd('/').ToLowerInvariant();

                if (path == "/station")
                {
                    await Shell.Current.GoToAsync(
                        $"{nameof(StationContentPage)}?code={Uri.EscapeDataString(code)}");
                }
                else if (path == "/share/station")
                {
                    await Shell.Current.GoToAsync(
                        $"{nameof(StationTeaserPage)}?code={Uri.EscapeDataString(code)}");
                }
            });
        }

        /// <summary>
        /// Liest einen Query-Parameter aus einer URI.
        /// </summary>
        /// <param name="uri">URI mit Query-String.</param>
        /// <param name="key">Name des gesuchten Parameters.</param>
        /// <returns>Wert des Parameters oder <c>null</c>, wenn der Parameter nicht vorhanden ist.</returns>
        private static string? GetQueryParam(Uri uri, string key)
        {
            var query = uri.Query.TrimStart('?');
            if (string.IsNullOrWhiteSpace(query))
                return null;

            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(kv[1]);
            }

            return null;
        }
    }
}
