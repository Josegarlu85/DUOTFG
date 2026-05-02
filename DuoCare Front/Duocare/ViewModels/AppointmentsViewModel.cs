using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Models;
using Duocare.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Duocare.ViewModels;

public partial class AppointmentsViewModel : ObservableObject
{
    [ObservableProperty] private string title = "";
    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty] private TimeSpan time = new(18, 0, 0);
    [ObservableProperty] private string notes = "";

    // ✅ Necesario para crear citas en backend (tu API exige ReceiverId)
    [ObservableProperty] private string receiverEmail = "";

    [ObservableProperty] private string selectedLocationText = "Toca el mapa para elegir ubicación";
    [ObservableProperty] private bool hasSelectedLocation;

    public double? SelectedLat { get; private set; }
    public double? SelectedLon { get; private set; }

    public ObservableCollection<Appointment> Appointments { get; } = new();

    private readonly ApiServices _api = new ApiServices();

    public AppointmentsViewModel()
    {
        _ = RefreshAsync();
    }

    // ✅ SE LLAMA AL TOCAR EL MAPA (DIRECCIÓN SIN API KEY)
    public async Task SetSelectedLocationAsync(double lat, double lon)
    {
        SelectedLat = lat;
        SelectedLon = lon;
        HasSelectedLocation = true;

        SelectedLocationText = "Buscando dirección…";

        try
        {
            using var http = new HttpClient();

            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "DuocareApp/1.0 (contact: prueba@duocare.app)"
            );

            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(CultureInfo.InvariantCulture);

            var url =
                $"https://nominatim.openstreetmap.org/reverse" +
                $"?lat={latStr}&lon={lonStr}&format=json&addressdetails=1";

            var resp = await http.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
            {
                SelectedLocationText =
                    $"Sin dirección: HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}\n" +
                    $"({lat:F5}, {lon:F5})";
                return;
            }

            var result = await resp.Content.ReadFromJsonAsync<NominatimResult>();

            if (result != null && !string.IsNullOrWhiteSpace(result.DisplayName))
            {
                SelectedLocationText =
                    $"{result.DisplayName}\n({lat:F5}, {lon:F5})";
            }
            else
            {
                SelectedLocationText =
                    $"Dirección no encontrada\n({lat:F5}, {lon:F5})";
            }
        }
        catch (Exception ex)
        {
            SelectedLocationText =
                $"Sin dirección (error)\n{ex.Message}\n({lat:F5}, {lon:F5})";
        }
    }

    // ✅ Cargar desde BD (API)
    [RelayCommand]
    public async Task RefreshAsync()
    {
        try
        {
            var page = await _api.GetMyAppointmentsAsync(1, 20);

            Appointments.Clear();

            foreach (var a in page.Data)
            {
                Appointments.Add(new Appointment
                {
                    Id = a.Id.ToString(),
                    Title = "Cita",
                    Date = a.Date.Date,
                    Time = a.Date.TimeOfDay,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    LocationText = $"{a.Latitude:F5}, {a.Longitude:F5}",
                    Notes = $"Estado: {a.Status}"
                });
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"No se pudieron cargar las citas del servidor.\n{ex.Message}",
                "OK");
        }
    }

    // ✅ Crear cita en BD (API)
    [RelayCommand]
    private async Task Save()
    {
        if (!HasSelectedLocation || SelectedLat == null || SelectedLon == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Falta ubicación",
                "Toca el mapa para seleccionar el lugar exacto.",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ReceiverEmail))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Falta receptor",
                "Introduce el email del receptor.",
                "OK");
            return;
        }

        try
        {
            // 1) Resolver receptor email -> id (UsersController)
            var receiver = await _api.FindUserByEmailAsync(ReceiverEmail.Trim());
            var receiverId = receiver.Id;

            // 2) Date + Time -> DateTime
            var when = Date.Date.Add(Time);

            // 3) Crear en backend (AppointmentsController)
            var created = await _api.CreateAppointmentAsync(
                new AppointmentCreateRequest(receiverId, when, SelectedLat.Value, SelectedLon.Value)
            );

            await Application.Current.MainPage.DisplayAlert(
                "OK",
                "Cita creada y guardada en el servidor.",
                "OK");

            // 4) Recargar desde BD
            await RefreshAsync();

            // Limpieza
            Title = "";
            Notes = "";
            ReceiverEmail = "";
            HasSelectedLocation = false;
            SelectedLocationText = "Toca el mapa para elegir ubicación";
            SelectedLat = null;
            SelectedLon = null;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Modelo Nominatim
    public class NominatimResult
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}