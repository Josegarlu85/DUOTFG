using Duocare.ViewModels;

namespace Duocare.Views;

public partial class CalendarPage : ContentPage
{
    public CalendarPage()
    {
        InitializeComponent();
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        if (BindingContext is CalendarViewModel vm)
        {
            vm.ChangeDateCommand.Execute(e.NewDate);
        }
    }
}
