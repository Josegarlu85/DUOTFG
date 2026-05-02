namespace Duocare.Models;


public class Appointment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public TimeSpan Time { get; set; } = new(18, 0, 0);

    // Texto visible (puede ser coordenadas o una dirección si luego reverse-geocoding)
    public string LocationText { get; set; } = string.Empty;

    // Coordenadas exactas seleccionadas en el mapa
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string Notes { get; set; } = string.Empty;
}