using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Hauptseite (AppShell) als Einstiegspunkt für Navigation
            MainPage = serviceProvider.GetRequiredService<AppShell>();
        }

        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            await Dispatcher.DispatchAsync(async () =>
            {
                // Sicherheit: nur eigene Domain
                if (!uri.Host.Equals("geschichtstour-backend.azurewebsites.net", StringComparison.OrdinalIgnoreCase))
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

        private static string? GetQueryParam(Uri uri, string key)
        {
            var query = uri.Query.TrimStart('?');
            if (string.IsNullOrWhiteSpace(query)) return null;

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
