using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using Windeck.Geschichtstour.Mobile.Behaviors;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Code-Behind fuer die Kartenansicht aller Stationen.
/// </summary>
public partial class StationsMapPage : ContentPage
{
    private readonly StationsMapViewModel _viewModel;
    private readonly MapView _mapView;
    private bool _initialized;

    /// <summary>
    /// Initialisiert eine neue Instanz von StationsMapPage.
    /// </summary>
    public StationsMapPage(StationsMapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Debug-Overlays global aus
        LoggingWidget.ShowLoggingInMap = ActiveMode.No;
        Performance.DefaultIsActive = ActiveMode.No;

        _mapView = new MapView
        {
            IsZoomButtonVisible = true,
            IsNorthingButtonVisible = false,
            IsMyLocationButtonVisible = false,
            MyLocationEnabled = false,
            MyLocationFollow = false
        };

        // ? MapView in XAML hosten (NICHT Content = _mapView)
        MapHost.Content = _mapView;

        // ? Behavior sicher an diese MapView hängen
        var bridge = new MapsuiMapViewBridgeBehavior
        {
            FitPinsOnFirstLoad = true,
            FitPaddingFactor = 0.20
        };

        // Wichtig: BindingContext setzen, damit Bindings sicher funktionieren
        bridge.BindingContext = _viewModel;

        bridge.SetBinding(MapsuiMapViewBridgeBehavior.ItemsSourceProperty, nameof(StationsMapViewModel.Pins));
        bridge.SetBinding(MapsuiMapViewBridgeBehavior.SelectedPinProperty, new Binding(nameof(StationsMapViewModel.SelectedPin), mode: BindingMode.TwoWay));
        bridge.SetBinding(MapsuiMapViewBridgeBehavior.IsMapBusyProperty, new Binding(nameof(StationsMapViewModel.IsMapBusy), mode: BindingMode.TwoWay));
        bridge.SetBinding(MapsuiMapViewBridgeBehavior.IsViewportInitializedProperty, new Binding(nameof(StationsMapViewModel.IsViewportInitialized), mode: BindingMode.TwoWay));
        bridge.SetBinding(MapsuiMapViewBridgeBehavior.IsPinsSynchronizedProperty, new Binding(nameof(StationsMapViewModel.IsPinsSynchronized), mode: BindingMode.TwoWay));

        _mapView.Behaviors.Add(bridge);

        // Map + OSM
        var map = new Mapsui.Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer("Windeck.Geschichtstour/1.0 (mailto:tourismuswindeck@gmail.com)"));

        // Startup View: Windeck (Lon,Lat)
        var center = SphericalMercator.FromLonLat(new MPoint(7.560, 50.792));

        var zoomIndex = 15;
        if (map.Navigator.Resolutions.Count > 0)
            zoomIndex = Math.Min(zoomIndex, map.Navigator.Resolutions.Count - 1);

        var resolution = map.Navigator.Resolutions.Count > 0
            ? map.Navigator.Resolutions[zoomIndex]
            : 1;

        map.Navigator.CenterOnAndZoomTo(center, resolution);

        _mapView.Map = map;
    }

    /// <summary>
    /// Wird beim Anzeigen der Seite aufgerufen und startet Initialisierungslogik.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized) return;
        _initialized = true;

        await _viewModel.LoadStationsAsync();
    }
}


