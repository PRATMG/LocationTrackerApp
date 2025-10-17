using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;           // ✅ for Map, Pin, Polyline
using LocationTrackerApp.Services;
using LocationTrackerApp.Models;
using Microsoft.Maui.Maps;

namespace LocationTrackerApp
{
    public partial class MainPage : ContentPage
    {
        private readonly LocationDatabase _db;
        int _count = 0;

        public MainPage(LocationDatabase db)
        {
            InitializeComponent();
            _db = db;
            _ = RefreshCountAsync();
        }

        private async Task RefreshCountAsync()
        {
            var c = await _db.CountAsync();
            LocationLabel.Text = $"Saved points: {c}";
        }

        // Template counter (kept so the project compiles if XAML still has the button)
        private void OnCounterClicked(object sender, EventArgs e)
        {
            _count++;
            var btn = (Button)sender;
            btn.Text = _count == 1 ? "Clicked 1 time" : $"Clicked {_count} times";
            SemanticScreenReader.Announce(btn.Text);
        }

        // Save current location to SQLite
        private async void OnTrackLocationClicked(object sender, EventArgs e)
        {
            if (!await RequestLocationPermissionAsync())
            {
                LocationLabel.Text = "Permission denied.";
                return;
            }

            var loc = await GetCurrentLocationAsync();
            if (loc is null)
            {
                LocationLabel.Text = "Unable to get location.";
                return;
            }

            await _db.AddAsync(new LocationEntry
            {
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
                TimestampUtc = DateTime.UtcNow
            });

            var count = await _db.CountAsync();
            LocationLabel.Text =
                $"Lat: {loc.Latitude.ToString("F5", CultureInfo.InvariantCulture)}, " +
                $"Lon: {loc.Longitude.ToString("F5", CultureInfo.InvariantCulture)}\n" +
                $"Saved points: {count}";
        }

        // Plot all saved points on the map (pins + red polyline) and center view
        private async void OnPlotClicked(object sender, EventArgs e)
        {
            await PlotAllAsync();
        }

        // Clear DB and map overlays
        private async void OnClearClicked(object sender, EventArgs e)
        {
            await _db.ClearAsync();
            LocationLabel.Text = "Saved points: 0";
            MapView.Pins.Clear();
            MapView.MapElements.Clear();
        }

        private async Task PlotAllAsync()
        {
            var points = await _db.GetAllAsync();

            MapView.Pins.Clear();
            MapView.MapElements.Clear();

            if (points.Count == 0)
            {
                LocationLabel.Text = "No saved points yet.";
                return;
            }

            // Optional: show current user location (iOS/Android)
            MapView.IsShowingUser = true;

            // Add pins for each saved point
            foreach (var p in points)
            {
                var pin = new Pin
                {
                    Label = p.TimestampUtc.ToLocalTime().ToString("g"),
                    Location = new Location(p.Latitude, p.Longitude),
                    Type = PinType.Place
                };
                MapView.Pins.Add(pin);
            }

            // Draw a red polyline connecting all points
            var polyline = new Polyline
            {
                StrokeColor = Colors.Red,
                StrokeWidth = 6
            };

            foreach (var p in points)
                polyline.Geopath.Add(new Location(p.Latitude, p.Longitude));

            MapView.MapElements.Add(polyline);

            // Center the map on the last point with a reasonable radius
            var last = points[^1];
            var center = new Location(last.Latitude, last.Longitude);
            MapView.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(2)));
        }

        private async Task<bool> RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }

        private async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                // Try cached first (fast)
                var cached = await Geolocation.GetLastKnownLocationAsync();
                if (cached != null) return cached;

                // Active request as fallback
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                return await Geolocation.GetLocationAsync(request);
            }
            catch
            {
                return null;
            }
        }
    }
}
