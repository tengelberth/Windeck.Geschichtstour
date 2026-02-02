using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Windeck.Geschichtstour.Mobile.ViewModels
{
    /// <summary>
    /// Basisklasse fuer Property-Change-Benachrichtigung und gemeinsame ViewModel-Funktionen.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Event, das ausgelöst wird, wenn eine Property geändert wird
        public event PropertyChangedEventHandler PropertyChanged;

        // Hilfsmethode zum Auslösen des PropertyChanged-Events
        /// <summary>
        /// Meldet Eigenschaftsaenderungen an gebundene UI-Elemente.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Generische Methode, die eine Property setzt und das PropertyChanged-Event auslöst
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Wenn der neue Wert gleich dem aktuellen Wert ist, keine Änderungen vornehmen
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // Wert setzen und PropertyChanged auslösen
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Eine grundlegende Property für die Ansicht
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }
    }

}


