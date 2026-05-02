using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Duocare.ViewModels;

public partial class PetEventViewModel : ObservableObject
{
    public List<string> EventTypes { get; } = new()
    {
        "Paseo",
        "Veterinario",
        "Estancia",
        "Cuidado especial"
    };

    [ObservableProperty] private string selectedEventType;
    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty] private TimeSpan time = new(18, 0, 0);
    [ObservableProperty] private string location;
    [ObservableProperty] private string notes;

    public IRelayCommand SaveCommand { get; }

    public PetEventViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
    }

    private async void OnSave()
    {
        await Application.Current.MainPage.DisplayAlert(
            "Evento guardado",
            "El evento de mascota ha sido creado correctamente.",
            "OK"
        );

        await Shell.Current.GoToAsync("..");
    }
}
