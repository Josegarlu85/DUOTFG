using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Duocare.Models;

public partial class PetModel : ObservableObject
{
    [ObservableProperty] private string name;
    [ObservableProperty] private string species;
    [ObservableProperty] private string breed;
    [ObservableProperty] private string age;
    [ObservableProperty] private bool vaccinesUpToDate;
    [ObservableProperty] private string notes;

    [ObservableProperty]
    private ObservableCollection<string> breedList = new();
}
