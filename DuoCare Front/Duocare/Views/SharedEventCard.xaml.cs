namespace Duocare.Views;

public partial class SharedEventCard : Frame
{
    public SharedEventCard()
    {
        InitializeComponent();
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("EventDetailsPage");
    }
}
