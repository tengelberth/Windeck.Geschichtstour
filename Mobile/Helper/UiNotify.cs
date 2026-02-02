using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Windeck.Geschichtstour.Mobile.Helpers;

public static class UiNotify
{
    //Branding
    private static readonly Color Primary = Color.FromArgb("#1953C6");

    // Moderner "Glass/Dark" Look
    private static readonly Color SnackBackground = Color.FromArgb("#1E1E1E"); // fast schwarz
    private static readonly Color TextColor = Colors.White;

    /// <summary>
    /// Kurzer Hinweis ohne Button (Toast).
    /// </summary>
    public static Task ToastAsync(string message)
    {
        return Toast.Make(
            message: message,
            duration: ToastDuration.Short,
            textSize: 14
        ).Show();
    }

    /// <summary>
    /// Snackbar mit Button (Action)
    /// </summary>
    public static Task SnackbarAsync(string message, string actionText, Action action, int seconds = 5)
    {
        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = SnackBackground,
            TextColor = TextColor,
            ActionButtonTextColor = Primary,
            CornerRadius = 14,
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14),
        };

        return Snackbar.Make(
             message: message,
            action: action,
            actionButtonText: actionText,
            duration: TimeSpan.FromSeconds(seconds),
            visualOptions: snackbarOptions
        ).Show();
    }

    /// <summary>
    /// Snackbar "Wiederholen?" -> true wenn der Nutzer klickt, sonst false (läuft aus).
    /// </summary>
    public static async Task<bool> SnackbarRetryAsync(string message, string actionText = "Wiederholen", int seconds = 5)
    {
        var tcs = new TaskCompletionSource<bool>();

        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = SnackBackground,
            TextColor = TextColor,
            ActionButtonTextColor = Primary,
            CornerRadius = 14,
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14),
        };

        var snackbar = Snackbar.Make(
            message: message,
            action: () => tcs.TrySetResult(true),
            actionButtonText: actionText,
            duration: TimeSpan.FromSeconds(seconds),
            visualOptions: snackbarOptions
        );

        await snackbar.Show();

        var finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(seconds)));
        return finished == tcs.Task && tcs.Task.Result;
    }
}
