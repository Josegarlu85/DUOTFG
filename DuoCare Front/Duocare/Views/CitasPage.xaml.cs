using Duocare.ViewModels;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;

namespace Duocare.Views;

public partial class CitasPage : ContentPage
{
    private Pin? _currentPin;

    public CitasPage()
    {
        InitializeComponent();

        // ✅ OpenStreetMap
        Map.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        // ✅ Tap en el mapa: poner pin + pedir dirección
        Map.Info += async (_, e) =>
        {
            if (e.WorldPosition == null || Map.Map == null)
                return;

            // Convertir coords del mapa (EPSG:3857) a lat/lon (WGS84)
            var lonLat = SphericalMercator.ToLonLat(e.WorldPosition.X, e.WorldPosition.Y);

            // 1) Quitar pin anterior (SIN Dispose)
            if (_currentPin != null)
            {
                Map.Pins.Remove(_currentPin);
                _currentPin = null;
            }

            // 2) Crear pin nuevo
            _currentPin = new Pin(Map)
            {
                Position = new Position(lonLat.lat, lonLat.lon),
                Label = "Ubicación seleccionada",
                Address = "Buscando dirección…",
                Type = PinType.Pin,
                Scale = 1.0f
            };

            // 3) Añadir y mostrar callout
            Map.Pins.Add(_currentPin);
            _currentPin.ShowCallout();

            // 4) Actualizar ViewModel (coordenadas + dirección)
            if (BindingContext is AppointmentsViewModel vm)
            {
                await vm.SetSelectedLocationAsync(lonLat.lat, lonLat.lon);

                // 5) Poner la dirección en el pin y volver a mostrar el callout
                _currentPin.Address = vm.SelectedLocationText;
                _currentPin.ShowCallout();
            }
        };

        _ = CenterOnUserAsync();
    }

    private async Task CenterOnUserAsync()
    {
        try
        {
            var loc = await Geolocation.Default.GetLastKnownLocationAsync();
            loc ??= await Geolocation.Default.GetLocationAsync();
            if (loc == null) return;

            var sm = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude);

            Map.Map?.Navigator?.CenterOnAndZoomTo(
                new MPoint(sm.x, sm.y),
                50);
        }
        catch
        {
            // Puede fallar si no hay permisos o en emulador
        }
    }
}
