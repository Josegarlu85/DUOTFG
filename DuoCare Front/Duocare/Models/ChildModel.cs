using CommunityToolkit.Mvvm.ComponentModel;

namespace Duocare.Models;

public partial class ChildModel : ObservableObject
{
    [ObservableProperty] private string name;
    [ObservableProperty] private string age;
    [ObservableProperty] private string school;
    [ObservableProperty] private string notes;
}
