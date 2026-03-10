using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Views;
namespace Windeck.Geschichtstour.Mobile.ViewModels
{
    /// <summary>
    /// Steuert die Startseite inklusive Codesuche und Hauptnavigation.
    /// </summary>
    public class HomeViewModel : BaseViewModel
    {
        private string _code = string.Empty;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }
        public Command ShowStationCommand { get; }
        public Command ShowAllStationsCommand { get; }
        public Command ShowAllToursCommand { get; }
        public Command ScanQrCommand { get; }

        // Konstruktor des ViewModels
        /// <summary>
        /// Initialisiert eine neue Instanz von HomeViewModel.
        /// </summary>
        public HomeViewModel()
        {
            ShowStationCommand = new Command(OnShowStationClicked);
            ShowAllStationsCommand = new Command(OnShowAllStationsClicked);
            ShowAllToursCommand = new Command(OnShowAllToursClicked);
            ScanQrCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(QrScannerPage));
            });
        }

        // Methode zum Anzeigen der Station (wird aufgerufen, wenn der Benutzer den "Station anzeigen"-Button klickt)
        /// <summary>
        /// Validiert den eingegebenen Code und oeffnet die passende Station.
        /// </summary>
        private async void OnShowStationClicked()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                await UiNotify.ToastAsync("Bitte gib einen Code ein.");
                return;
            }
            await Shell.Current.GoToAsync($"{nameof(StationContentPage)}?code={Uri.EscapeDataString(Code.Trim())}");
        }

        // Methode zum Anzeigen aller Stationen (wird aufgerufen, wenn der Benutzer den "Alle Stationen anzeigen"-Button klickt)
        /// <summary>
        /// Navigiert zur Uebersicht aller Stationen.
        /// </summary>
        private async void OnShowAllStationsClicked()
        {
            await Shell.Current.GoToAsync("//stations");
        }

        // Methode zum Anzeigen aller Touren (wird aufgerufen, wenn der Benutzer den "Alle Touren anzeigen"-Button klickt)
        /// <summary>
        /// Navigiert zur Uebersicht aller Touren.
        /// </summary>
        private async void OnShowAllToursClicked()
        {
            await Shell.Current.GoToAsync("//tours");
        }
    }
}
