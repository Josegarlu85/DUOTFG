using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Models;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Duocare.ViewModels;

public partial class ChildEventPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string location;

    [ObservableProperty]
    private DateTime date = DateTime.Today;

    private readonly CalendarViewModel calendar;

    public ChildEventPageViewModel(CalendarViewModel calendarVm)
    {
        calendar = calendarVm;
    }

    [RelayCommand]
    private async Task Save()
    {
        var ev = new CalendarEvent
        {
            Title = Title,
            Location = Location,
            Date = Date,
            IsForChild = true
        };

        calendar.AddEvent(ev);

        await Shell.Current.GoToAsync("..");
    }
}
