using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;

namespace Duocare.ViewModels;

public partial class ProfileSetupViewModel : ObservableObject
{
    public IRelayCommand PetCommand { get; }
    public IRelayCommand ChildCommand { get; }
    public IRelayCommand BothCommand { get; }

    public ProfileSetupViewModel()
    {
        PetCommand = new RelayCommand(OnPetSelected);
        ChildCommand = new RelayCommand(OnChildSelected);
        BothCommand = new RelayCommand(OnBothSelected);
    }

    private async void OnPetSelected()
    {
        // ❌ NO marcar ProfileCompleted aquí
        await Shell.Current.GoToAsync("PetFormPage");
    }

    private async void OnChildSelected()
    {
        await Shell.Current.GoToAsync("ChildFormPage");
    }

    private async void OnBothSelected()
    {
        await Shell.Current.GoToAsync("CombinedFormPage");
    }
}