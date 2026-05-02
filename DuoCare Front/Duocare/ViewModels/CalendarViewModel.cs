using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Models;
using System.Collections.ObjectModel;

namespace Duocare.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    // ============================
    // PROPIEDADES PRINCIPALES
    // ============================

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    // ⭐ FECHA BONITA PARA MOSTRAR EN LA UI
    public string SelectedDateFormatted =>
        SelectedDate.ToString("dddd d 'de' MMMM").ToUpper();

    // ⭐ LISTAS INTERNAS
    [ObservableProperty]
    private ObservableCollection<CalendarEvent> childEventsForSelectedDate = new();

    [ObservableProperty]
    private ObservableCollection<CalendarEvent> petEventsForSelectedDate = new();

    // ⭐ ALIAS PARA EL XAML (ChildEvents y PetEvents)
    public ObservableCollection<CalendarEvent> ChildEvents => ChildEventsForSelectedDate;
    public ObservableCollection<CalendarEvent> PetEvents => PetEventsForSelectedDate;

    // ⭐ MENSAJE “No hay eventos de mascotas”
    public bool NoPetEvents => PetEventsForSelectedDate.Count == 0;

    // ⭐ TODOS LOS EVENTOS
    public ObservableCollection<CalendarEvent> AllEvents { get; set; } = new();

    public CalendarViewModel()
    {
        LoadMockEvents();
        LoadEventsForSelectedDate();
    }

    // ============================
    // EVENTOS DE PRUEBA
    // ============================
    private void LoadMockEvents()
    {
        AllEvents.Add(new CalendarEvent
        {
            Title = "Recogida del niño",
            Date = DateTime.Today,
            Location = "Entrada del colegio",
            IsForChild = true
        });

        AllEvents.Add(new CalendarEvent
        {
            Title = "Cita con el veterinario",
            Date = DateTime.Today.AddDays(1),
            Location = "Clínica veterinaria",
            IsForPet = true
        });
    }

    // ============================
    // CAMBIAR FECHA
    // ============================
    [RelayCommand]
    private void ChangeDate(DateTime newDate)
    {
        SelectedDate = newDate;
        LoadEventsForSelectedDate();

        // ⭐ Actualiza la fecha bonita
        OnPropertyChanged(nameof(SelectedDateFormatted));
    }

    // ============================
    // CARGAR EVENTOS DEL DÍA
    // ============================
    public void LoadEventsForSelectedDate()
    {
        ChildEventsForSelectedDate.Clear();
        PetEventsForSelectedDate.Clear();

        foreach (var ev in AllEvents.Where(e => e.Date.Date == SelectedDate.Date))
        {
            if (ev.IsForChild)
                ChildEventsForSelectedDate.Add(ev);

            if (ev.IsForPet)
                PetEventsForSelectedDate.Add(ev);
        }

        // ⭐ Actualiza visibilidad del mensaje “No hay eventos…”
        OnPropertyChanged(nameof(NoPetEvents));
    }

    // ============================
    // NAVEGACIÓN
    // ============================

    // 👶 Crear evento de niño
    [RelayCommand]
    private async Task CreateChildEvent()
    {
        await Shell.Current.GoToAsync("ChildEventPage");
    }

    // 🐾 Crear evento de mascota
    [RelayCommand]
    private async Task CreatePetEvent()
    {
        await Shell.Current.GoToAsync("PetEventPage");
    }

    // 🔍 Ver detalles
    [RelayCommand]
    private async Task ViewEventDetails(CalendarEvent ev)
    {
        if (ev == null)
            return;

        string type = ev.IsForChild ? "Child" :
                      ev.IsForPet ? "Pet" : "Unknown";

        var parameters = new Dictionary<string, object>
        {
            { "Title", ev.Title },
            { "Date", ev.Date.ToString("dddd, d 'de' MMMM") },
            { "Location", ev.Location },
            { "Type", type }
        };

        await Shell.Current.GoToAsync("EventDetailsPage", parameters);
    }

    // 🗑 Eliminar evento
    [RelayCommand]
    private void DeleteEvent(CalendarEvent ev)
    {
        if (ev == null)
            return;

        AllEvents.Remove(ev);
        LoadEventsForSelectedDate();
    }

    // ⭐ MÉTODO QUE USAN LOS FORMULARIOS PARA AÑADIR EVENTOS
    public void AddEvent(CalendarEvent ev)
    {
        AllEvents.Add(ev);
        LoadEventsForSelectedDate();
    }
}
