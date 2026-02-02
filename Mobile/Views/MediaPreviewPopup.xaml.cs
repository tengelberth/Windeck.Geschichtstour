using CommunityToolkit.Maui.Views;

namespace Windeck.Geschichtstour.Mobile.Views;

public partial class MediaPreviewPopup : Popup
{
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

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
