using Windeck.Geschichtstour.Mobile.Helper;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Views;

    namespace Windeck.Geschichtstour.Mobile.ViewModels
    {
        public class HomeViewModel : BaseViewModel
        {
            private string _code;

            // Property für das Code-Eingabefeld
            public string Code
            {
                get => _code;
                set => SetProperty(ref _code, value);
            }

        // Commands
        public Command ShowStationCommand { get; }
            public Command ShowAllStationsCommand { get; }
            public Command ShowAllToursCommand { get; }
            public Command ScanQrCommand { get; }

        // Konstruktor des ViewModels
        public HomeViewModel()
            {
                // Initialisiere die Commands und weise ihnen die entsprechenden Methoden zu
                ShowStationCommand = new Command(OnShowStationClicked);
                ShowAllStationsCommand = new Command(OnShowAllStationsClicked);
                ShowAllToursCommand = new Command(OnShowAllToursClicked);
                ScanQrCommand = new Command(async () =>
                {
                    await Shell.Current.GoToAsync(nameof(QrScannerPage));
                });
        }

            // Methode zum Anzeigen der Station (wird aufgerufen, wenn der Benutzer den "Station anzeigen"-Button klickt)
            private async void OnShowStationClicked()
            {
                // Überprüfe, ob der Code eingegeben wurde
                if (string.IsNullOrWhiteSpace(Code))
                {
                await UiNotify.ToastAsync("Bitte gib einen Code ein.");
                return;
                }

                // Navigiere zur StationContentPage und übergebe den Code als Query-Parameter
                // `nameof()` stellt sicher, dass die Seite korrekt referenziert wird, auch wenn der Name der Seite geändert wird
                await Shell.Current.GoToAsync($"{nameof(StationContentPage)}?code={Uri.EscapeDataString(Code.Trim())}");
            }

            // Methode zum Anzeigen aller Stationen (wird aufgerufen, wenn der Benutzer den "Alle Stationen anzeigen"-Button klickt)
            private async void OnShowAllStationsClicked()
            {
                // Navigiere zur Stations-Seite
                await Shell.Current.GoToAsync("//stations");
            }

            // Methode zum Anzeigen aller Touren (wird aufgerufen, wenn der Benutzer den "Alle Touren anzeigen"-Button klickt)
            private async void OnShowAllToursClicked()
            {
                // Navigiere zur Touren-Seite
                await Shell.Current.GoToAsync("//tours");
            }
        }
    }
