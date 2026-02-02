using CommunityToolkit.Maui.Views;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Popup zur Vollbildvorschau einzelner Medieninhalte.
/// </summary>
public partial class MediaPreviewPopup : Popup
{
    /// <summary>
    /// Initialisiert eine neue Instanz von MediaPreviewPopup.
    /// </summary>
    public MediaPreviewPopup(string imageUrl)
    {
        InitializeComponent();

        PreviewImage.Source = imageUrl;

        // fast fullscreen
        var info = DeviceDisplay.MainDisplayInfo;
        var widthDp = info.Width / info.Density;
        var heightDp = info.Height / info.Density;

        Size = new Size(widthDp, heightDp);
    }

    /// <summary>
    /// Schliesst das Popup nach einem Klick auf die Schliessen-Aktion.
    /// </summary>
    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}


