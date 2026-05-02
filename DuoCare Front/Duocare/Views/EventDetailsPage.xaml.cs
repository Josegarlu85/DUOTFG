namespace Duocare.Views;

[QueryProperty(nameof(EventTitle), "Title")]
[QueryProperty(nameof(EventDate), "Date")]
[QueryProperty(nameof(EventLocation), "Location")]
[QueryProperty(nameof(EventType), "Type")]
public partial class EventDetailsPage : ContentPage
{
    private string eventTitle;
    public string EventTitle
    {
        get => eventTitle;
        set { eventTitle = value; OnPropertyChanged(); }
    }

    private string eventDate;
    public string EventDate
    {
        get => eventDate;
        set { eventDate = value; OnPropertyChanged(); }
    }

    private string eventLocation;
    public string EventLocation
    {
        get => eventLocation;
        set { eventLocation = value; OnPropertyChanged(); }
    }

    private string eventType;
    public string EventType
    {
        get => eventType;
        set
        {
            eventType = value;
            OnPropertyChanged();
            UpdateVisibility();
        }
    }

    public bool IsChildEvent { get; set; }
    public bool IsPetEvent { get; set; }

    private void UpdateVisibility()
    {
        IsChildEvent = EventType == "Child";
        IsPetEvent = EventType == "Pet";

        OnPropertyChanged(nameof(IsChildEvent));
        OnPropertyChanged(nameof(IsPetEvent));
    }

    public EventDetailsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}
