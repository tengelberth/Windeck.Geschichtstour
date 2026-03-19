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

        private const double LoadingFeedbackStepSeconds = 5;

        private CancellationTokenSource? _loadingFeedbackCts;
        private bool _isBusy;
        private string _loadingMessage = string.Empty;
        private string _loadingElapsedText = string.Empty;

        /// <summary>
        /// Meldet Eigenschaftsänderungen an gebundene UI-Elemente.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Setzt eine Eigenschaft und löst bei änderung eine Benachrichtigung aus.
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

        /// <summary>
        /// Kennzeichnet laufende Lade- oder Hintergrundprozesse fuer die UI.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Enthält die aktuell sichtbare Ladebotschaft fuer den Nutzer.
        /// </summary>
        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        /// <summary>
        /// Enthält die laufende Wartezeit als kurzen Zusatztext fuer die UI.
        /// </summary>
        public string LoadingElapsedText
        {
            get => _loadingElapsedText;
            set => SetProperty(ref _loadingElapsedText, value);
        }

        /// <summary>
        /// Startet rotierende Statusmeldungen fuer längere Ladephasen.
        /// </summary>
        protected void StartLoadingFeedback(params string[] messages)
        {
            StopLoadingFeedback();

            string[] loadingMessages = messages
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();

            if (loadingMessages.Length == 0)
            {
                loadingMessages = ["Ich sammle die Daten zusammen..."];
            }

            CancellationTokenSource cts = new();
            _loadingFeedbackCts = cts;
            UpdateLoadingFeedback(loadingMessages[0], TimeSpan.Zero);
            _ = RunLoadingFeedbackAsync(loadingMessages, cts.Token);
        }

        /// <summary>
        /// Stoppt aktive Statusmeldungen und leert die Anzeige wieder.
        /// </summary>
        protected void StopLoadingFeedback()
        {
            CancellationTokenSource? cts = Interlocked.Exchange(ref _loadingFeedbackCts, null);
            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                }
                finally
                {
                    cts.Dispose();
                }
            }

            UpdateLoadingFeedback(string.Empty, null);
        }

        /// <summary>
        /// Aktualisiert die sichtbaren Ladetexte in festen Zeitabständen.
        /// </summary>
        private async Task RunLoadingFeedbackAsync(IReadOnlyList<string> messages, CancellationToken cancellationToken)
        {
            DateTime startedAt = DateTime.UtcNow;
            int messageIndex = 0;
            double secondsPerMessage = LoadingFeedbackStepSeconds;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    TimeSpan elapsed = DateTime.UtcNow - startedAt;
                    if (messages.Count > 1 && elapsed.TotalSeconds >= secondsPerMessage)
                    {
                        messageIndex = Math.Min((int)(elapsed.TotalSeconds / secondsPerMessage), messages.Count - 1);
                    }

                    UpdateLoadingFeedback(messages[messageIndex], elapsed);
                }
            }
            catch (OperationCanceledException)
            {
                // Normales Ende beim Beenden des Ladezustands.
            }
        }

        /// <summary>
        /// Setzt Ladebotschaft und Wartezeit thread-sicher auf dem UI-Thread.
        /// </summary>
        private void UpdateLoadingFeedback(string message, TimeSpan? elapsed)
        {
            void Apply()
            {
                LoadingMessage = message;
                LoadingElapsedText = elapsed.HasValue
                    ? $"Wartezeit: {Math.Max(1, (int)elapsed.Value.TotalSeconds)} s"
                    : string.Empty;
            }

            if (MainThread.IsMainThread)
            {
                Apply();
                return;
            }

            MainThread.BeginInvokeOnMainThread(Apply);
        }
    }
}
