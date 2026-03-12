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
    public MediaPreviewPopup(string imageUrl, string? caption = null)
    {
        InitializeComponent();

        PreviewImage.Source = imageUrl;

        if (!string.IsNullOrWhiteSpace(caption))
        {
            CaptionLabel.Text = caption.Trim();
            CaptionBorder.IsVisible = true;
        }

        DisplayInfo info = DeviceDisplay.MainDisplayInfo;
        double widthDp = info.Width / info.Density;
        double heightDp = info.Height / info.Density;

        WidthRequest = widthDp;
        HeightRequest = heightDp;
    }

    /// <summary>
    /// Schliesst das Popup nach einem Klick auf die Schliessen-Aktion.
    /// </summary>
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}
