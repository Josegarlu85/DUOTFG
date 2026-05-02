namespace Duocare.Models;

public class SharedEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string Location { get; set; } = string.Empty;
    public string ParticipantA { get; set; } = string.Empty;
    public string ParticipantB { get; set; } = string.Empty;
    public bool IsForChild { get; set; }
    public bool IsForPet { get; set; }
    public string Notes { get; set; } = string.Empty;
}
