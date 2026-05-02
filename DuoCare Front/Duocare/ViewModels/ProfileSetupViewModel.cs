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
        Preferences.Set("FichaSeleccionada", "Mascota");
        await Shell.Current.GoToAsync("PetFormPage");
    }

    private async void OnChildSelected()
    {
        Preferences.Set("FichaSeleccionada", "Niño");
        await Shell.Current.GoToAsync("ChildFormPage");
    }

    private async void OnBothSelected()
    {
        Preferences.Set("FichaSeleccionada", "Ambos");
        await Shell.Current.GoToAsync("CombinedFormPage");
    }
}