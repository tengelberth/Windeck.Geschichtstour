using Windeck.Geschichtstour.Mobile.Helper;
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

        /// <summary>
        /// Validiert den eingegebenen Code, normalisiert freie Eingaben und öffnet die passende Station.
        /// </summary>
        private async void OnShowStationClicked()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                await UiNotify.ToastAsync("Bitte gib einen Code ein.");
                return;
            }

            var normalizedCode = QrCodeParser.TryNormalizeCode(Code);
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                await UiNotify.ToastAsync("Der Stationscode konnte nicht erkannt werden.");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(StationContentPage)}?code={Uri.EscapeDataString(normalizedCode)}");
        }

        /// <summary>
        /// Navigiert zur Übersicht aller Stationen.
        /// </summary>
        private async void OnShowAllStationsClicked()
        {
            await Shell.Current.GoToAsync("//stations");
        }

        /// <summary>
        /// Navigiert zur Übersicht aller Touren.
        /// </summary>
        private async void OnShowAllToursClicked()
        {
            await Shell.Current.GoToAsync("//tours");
        }
    }
}
