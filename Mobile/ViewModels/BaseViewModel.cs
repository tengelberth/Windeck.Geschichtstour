using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Windeck.Geschichtstour.Mobile.ViewModels
{
    /// <summary>
    /// Basisklasse fuer Property-Change-Benachrichtigung und gemeinsame ViewModel-Funktionen.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Meldet Eigenschaftsaenderungen an gebundene UI-Elemente.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Setzt eine Eigenschaft und loest bei Aenderung eine Benachrichtigung aus.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
