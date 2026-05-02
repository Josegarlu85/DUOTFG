namespace Duocare.Models;

public class CalendarEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsForChild { get; set; }
    public bool IsForPet { get; set; }
}
