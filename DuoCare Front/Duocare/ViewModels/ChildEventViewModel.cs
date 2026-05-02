using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ChildEventViewModel : ObservableObject
{
    public List<string> EventTypes { get; } = new()
    {
        "Recogida",
        "Entrega",
        "Actividad escolar",
        "Cita médica",
        "Otro"
    };

    [ObservableProperty] private string selectedEventType;
    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty] private TimeSpan time = new(18, 0, 0);
    [ObservableProperty] private string location;
    [ObservableProperty] private string notes;

    public IRelayCommand SaveCommand { get; }

    public ChildEventViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
    }

    private async void OnSave()
    {
        await Application.Current.MainPage.DisplayAlert(
            "Evento guardado",
            "El evento del niño ha sido creado correctamente.",
            "OK"
        );

        await Shell.Current.GoToAsync("..");
    }
}
