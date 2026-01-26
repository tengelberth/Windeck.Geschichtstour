namespace Windeck.Geschichtstour.Mobile.Controls;

public class ZoomContainer : ContentView
{
    const double MIN_SCALE = 1;
    const double MAX_SCALE = 8;

    const double DOUBLE_TAP_ZOOM = 3.5;
    const uint DOUBLE_TAP_ANIM_MS = 140;

    double _currentScale = 1;

    double _xOffset = 0;
    double _yOffset = 0;

    double _containerWidth = 0;
    double _containerHeight = 0;

    public ZoomContainer()
    {
        SizeChanged += (_, __) =>
        {
            _containerWidth = Width;
            _containerHeight = Height;
        };

        // 1-Finger bewegen
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(pan);

        // Double Tap = Zoom Toggle
        var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTap.Tapped += OnDoubleTapped;
        GestureRecognizers.Add(doubleTap);
    }

    async void OnDoubleTapped(object? sender, EventArgs e)
    {
        if (Content == null) return;

        Content.AnchorX = 0.5;
        Content.AnchorY = 0.5;

        // Toggle: reinzoomen <-> reset
        if (_currentScale <= 1.01)
        {
            _currentScale = DOUBLE_TAP_ZOOM;
            await ApplyTransformAsync(_currentScale, 0, 0, animate: true);
        }
        else
        {
            await ApplyTransformAsync(1, 0, 0, animate: true);
            _currentScale = 1;
            _xOffset = 0;
            _yOffset = 0;
        }
    }

    void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (Content == null) return;

        // nur bewegen wenn gezoomt
        if (_currentScale <= 1.01) return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                {
                    var newX = _xOffset + e.TotalX;
                    var newY = _yOffset + e.TotalY;

                    ClampTranslation(ref newX, ref newY);

                    Content.TranslationX = newX;
                    Content.TranslationY = newY;
                    break;
                }

            case GestureStatus.Completed:
                {
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
                }
        }
    }

    async Task ApplyTransformAsync(double scale, double tx, double ty, bool animate)
    {
        if (Content == null) return;

        // Clamp Scale
        scale = Math.Max(MIN_SCALE, Math.Min(MAX_SCALE, scale));

        if (animate)
        {
            await Task.WhenAll(
                Content.ScaleTo(scale, DOUBLE_TAP_ANIM_MS, Easing.CubicOut),
                Content.TranslateTo(tx, ty, DOUBLE_TAP_ANIM_MS, Easing.CubicOut)
            );
        }
        else
        {
            Content.Scale = scale;
            Content.TranslationX = tx;
            Content.TranslationY = ty;
        }
    }

    void ClampTranslation(ref double x, ref double y)
    {
        if (_containerWidth <= 0 || _containerHeight <= 0)
            return;

        // verhindert wegziehen: Grenzen abhängig vom Zoom
        var maxX = (_containerWidth * (_currentScale - 1)) / 2;
        var maxY = (_containerHeight * (_currentScale - 1)) / 2;

        x = Math.Max(-maxX, Math.Min(maxX, x));
        y = Math.Max(-maxY, Math.Min(maxY, y));
    }
}
