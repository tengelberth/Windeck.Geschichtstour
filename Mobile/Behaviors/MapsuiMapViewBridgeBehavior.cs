using Mapsui;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using System.Collections.Specialized;

namespace Windeck.Geschichtstour.Mobile.Behaviors;

/// <summary>
/// Synchronisiert Pins, Auswahl und Busy-Status zwischen MapView und ViewModel.
/// </summary>
public class MapsuiMapViewBridgeBehavior : Behavior<MapView>
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable<Pin>),
            typeof(MapsuiMapViewBridgeBehavior),
            default(IEnumerable<Pin>),
            propertyChanged: (b, o, n) => ((MapsuiMapViewBridgeBehavior)b).OnItemsSourceChanged(o as IEnumerable<Pin>, n as IEnumerable<Pin>));

    public IEnumerable<Pin>? ItemsSource
    {
        get => (IEnumerable<Pin>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty SelectedPinProperty =
        BindableProperty.Create(
            nameof(SelectedPin),
            typeof(Pin),
            typeof(MapsuiMapViewBridgeBehavior),
            default(Pin),
            BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((MapsuiMapViewBridgeBehavior)b).OnSelectedPinVmChanged(o as Pin, n as Pin));

    public Pin? SelectedPin
    {
        get => (Pin?)GetValue(SelectedPinProperty);
        set => SetValue(SelectedPinProperty, value);
    }

    public static readonly BindableProperty IsMapBusyProperty =
        BindableProperty.Create(
            nameof(IsMapBusy),
            typeof(bool),
            typeof(MapsuiMapViewBridgeBehavior),
            false,
            BindingMode.TwoWay);

    public bool IsMapBusy
    {
        get => (bool)GetValue(IsMapBusyProperty);
        set => SetValue(IsMapBusyProperty, value);
    }

    public static readonly BindableProperty IsViewportInitializedProperty =
        BindableProperty.Create(
            nameof(IsViewportInitialized),
            typeof(bool),
            typeof(MapsuiMapViewBridgeBehavior),
            false,
            BindingMode.TwoWay);

    public bool IsViewportInitialized
    {
        get => (bool)GetValue(IsViewportInitializedProperty);
        set => SetValue(IsViewportInitializedProperty, value);
    }

    public bool FitPinsOnFirstLoad { get; set; } = true;

    /// <summary>Padding relativ zur Extent-Breite/Höhe (z.B. 0.2 = 20%)</summary>
    public double FitPaddingFactor { get; set; } = 0.20;

    private MapView? _mapView;
    private INotifyCollectionChanged? _notifyPins;
    private Mapsui.Map? _map;

    private CancellationTokenSource? _syncCts;

    private bool _ignoreSelected;
    private bool _didInitialFit;
    private bool _pendingFit;

    /// <summary>
    /// Initialisiert die Behavior beim Anhaengen an die MapView.
    /// </summary>
    protected override void OnAttachedTo(MapView bindable)
    {
        base.OnAttachedTo(bindable);
        _mapView = bindable;

        bindable.SelectedPinChanged += MapView_SelectedPinChanged;
        bindable.PropertyChanged += MapView_PropertyChanged;

        // Fallback: sobald MAUI dem Control eine Größe gibt -> Viewport gilt als “initialisiert”
        bindable.SizeChanged += MapView_SizeChanged;

        AttachPinsCollection(ItemsSource);
        AttachMap(bindable.Map);

        // Falls Viewport bereits eine Größe hat (Event evtl. schon durch): sofort setzen
        TryMarkViewportInitialized();

        _ = DebouncedSyncPinsAsync();
    }


    /// <summary>
    /// Raeumt Ressourcen auf, wenn die Behavior von der MapView getrennt wird.
    /// </summary>
    protected override void OnDetachingFrom(MapView bindable)
    {
        base.OnDetachingFrom(bindable);

        bindable.SelectedPinChanged -= MapView_SelectedPinChanged;
        bindable.PropertyChanged -= MapView_PropertyChanged;

        DetachPinsCollection();
        DetachMap();

        _syncCts?.Cancel();
        _syncCts = null;

        _mapView = null;
        bindable.SizeChanged -= MapView_SizeChanged;

    }

    /// <summary>
    /// Verarbeitet Eigenschaftsaenderungen der MapView.
    /// </summary>
    private void MapView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapView.Map))
            AttachMap(_mapView?.Map);
    }

    /// <summary>
    /// Reagiert auf Aenderungen der Pin-Datenquelle.
    /// </summary>
    private void OnItemsSourceChanged(IEnumerable<Pin>? oldValue, IEnumerable<Pin>? newValue)
    {
        DetachPinsCollection();
        AttachPinsCollection(newValue);
        _ = DebouncedSyncPinsAsync();
    }

    /// <summary>
    /// Verknuepft die aktuelle Pin-Sammlung mit der Behavior und abonniert Aenderungen.
    /// </summary>
    private void AttachPinsCollection(IEnumerable<Pin>? source)
    {
        if (source is INotifyCollectionChanged incc)
        {
            _notifyPins = incc;
            _notifyPins.CollectionChanged += Pins_CollectionChanged;
        }
    }

    /// <summary>
    /// Entfernt die Verknuepfung zur Pin-Sammlung und beendet Ereignisabonnements.
    /// </summary>
    private void DetachPinsCollection()
    {
        if (_notifyPins != null)
            _notifyPins.CollectionChanged -= Pins_CollectionChanged;

        _notifyPins = null;
    }

    /// <summary>
    /// Reagiert auf Aenderungen der Pin-Sammlung und startet die Synchronisierung.
    /// </summary>
    private void Pins_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Debounce: verhindert 100x Clear/Add bei Bulk-Load
        _ = DebouncedSyncPinsAsync();
    }

    /// <summary>
    /// Synchronisiert Pins zeitlich entkoppelt, um haeufige Updates zu buendeln.
    /// </summary>
    private async Task DebouncedSyncPinsAsync()
    {
        _syncCts?.Cancel();
        var cts = _syncCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(120, cts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (cts.IsCancellationRequested) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_mapView == null) return;

            _mapView.Pins.Clear();

            if (ItemsSource != null)
            {
                foreach (var p in ItemsSource)
                    _mapView.Pins.Add(p);
            }

            _mapView.RefreshGraphics();

            // e) HIER rein:
            _mapView.ForceUpdate();

            // Auto-Fit (nur einmal) sobald Pins da sind
            if (FitPinsOnFirstLoad && !_didInitialFit && _mapView.Pins.Count > 0)
            {
                if (IsViewportInitialized)
                    FitToPins();
                else
                    _pendingFit = true;
            }
        });
    }


    /// <summary>
    /// Reagiert auf eine geaenderte Pin-Auswahl im ViewModel.
    /// </summary>
    private void OnSelectedPinVmChanged(Pin? oldPin, Pin? newPin)
    {
        if (_mapView == null) return;
        if (_ignoreSelected) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // VM -> View
            if (_mapView != null && _mapView.SelectedPin != newPin)
                _mapView.SelectedPin = newPin;
        });
    }

    /// <summary>
    /// Synchronisiert die Pin-Auswahl der MapView mit dem ViewModel.
    /// </summary>
    private void MapView_SelectedPinChanged(object? sender, SelectedPinChangedEventArgs e)
    {
        if (_mapView == null) return;

        // View -> VM
        _ignoreSelected = true;
        try
        {
            SelectedPin = _mapView.SelectedPin;
        }
        finally
        {
            _ignoreSelected = false;
        }
    }

    /// <summary>
    /// Verknuepft die Behavior mit der Karteninstanz und registriert erforderliche Ereignisse.
    /// </summary>
    private void AttachMap(Mapsui.Map? map)
    {
        if (ReferenceEquals(_map, map)) return;

        DetachMap();
        _map = map;
        if (_map == null) return;

        _map.ViewportInitialized += Map_ViewportInitialized;
        _map.DataChanged += Map_DataChanged;

        UpdateMapBusy();

        // wichtig: falls ViewportInitialized schon war
        MainThread.BeginInvokeOnMainThread(TryMarkViewportInitialized);
    }


    /// <summary>
    /// Loest die Verknuepfung zur Karteninstanz und entfernt Ereignisabonnements.
    /// </summary>
    private void DetachMap()
    {
        if (_map == null) return;

        _map.ViewportInitialized -= Map_ViewportInitialized;
        _map.DataChanged -= Map_DataChanged;
        _map = null;
    }

    /// <summary>
    /// Reagiert auf die Initialisierung des Karten-Viewports.
    /// </summary>
    private void Map_ViewportInitialized(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsViewportInitialized = true;

            if (_pendingFit && FitPinsOnFirstLoad && !_didInitialFit)
            {
                _pendingFit = false;
                FitToPins();
            }
        });
    }

    /// <summary>
    /// Reagiert auf Datenaenderungen der Karte und aktualisiert die Darstellung.
    /// </summary>
    private void Map_DataChanged(object? sender, DataChangedEventArgs e)
    {
        // Wird häufig gefeuert -> nur Busy neu ausrechnen
        UpdateMapBusy();
    }

    /// <summary>
    /// Aktualisiert den Busy-Status der Karte fuer die UI.
    /// </summary>
    private void UpdateMapBusy()
    {
        var map = _map;
        if (map == null) return;

        // Busy, wenn irgendein Layer gerade fetch/render macht
        var busy = map.Layers?.Any(l => l is ILayer layer && layer.Busy) == true;

        MainThread.BeginInvokeOnMainThread(() => IsMapBusy = busy);
    }

    /// <summary>
    /// Passt den Kartenausschnitt so an, dass alle relevanten Pins sichtbar sind.
    /// </summary>
    private void FitToPins()
    {
        if (_mapView?.Map == null) return;

        var pins = _mapView.Pins.ToList();
        if (pins.Count == 0) return;

        // 1 Pin: nur zentrieren (Zoom beibehalten)
        if (pins.Count == 1)
        {
            var p = pins[0];
            var center = SphericalMercator.FromLonLat(new MPoint(p.Position.Longitude, p.Position.Latitude));
            _mapView.Map.Navigator.CenterOnAndZoomTo(center, _mapView.Map.Navigator.Viewport.Resolution);
            _didInitialFit = true;
            return;
        }

        // Extent berechnen (OSM = EPSG:3857)
        var merc = pins.Select(p => SphericalMercator.FromLonLat(new MPoint(p.Position.Longitude, p.Position.Latitude))).ToList();

        var minX = merc.Min(m => m.X);
        var minY = merc.Min(m => m.Y);
        var maxX = merc.Max(m => m.X);
        var maxY = merc.Max(m => m.Y);

        var rect = new MRect(minX, minY, maxX, maxY);

        // Padding
        var padX = rect.Width * FitPaddingFactor;
        var padY = rect.Height * FitPaddingFactor;
        rect = rect.Grow(padX, padY);

        _mapView.Map.Navigator.ZoomToBox(rect);
        _didInitialFit = true;
    }

    /// <summary>
    /// Reagiert auf Groessenaenderungen der MapView und aktualisiert den Ausschnitt.
    /// </summary>
    private void MapView_SizeChanged(object? sender, EventArgs e)
    {
        TryMarkViewportInitialized();
    }

    /// <summary>
    /// Markiert den Viewport als initialisiert, sobald die Voraussetzungen erfuellt sind.
    /// </summary>
    private void TryMarkViewportInitialized()
    {
        if (_mapView?.Map == null) return;
        if (IsViewportInitialized) return;

        // MAUI Size ist der zuverlässigste Indikator
        if (_mapView.Width > 0 && _mapView.Height > 0)
        {
            IsViewportInitialized = true;

            if (_pendingFit && FitPinsOnFirstLoad && !_didInitialFit && _mapView.Pins.Count > 0)
            {
                _pendingFit = false;
                FitToPins();
            }
            return;
        }

        // Optional zusätzlich: Mapsui Viewport HasSize (falls stabil verfügbar)
        var vp = _mapView.Map.Navigator.Viewport;
        if (vp.HasSize())
            IsViewportInitialized = true;
    }


}




