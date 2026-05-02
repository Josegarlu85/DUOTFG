using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Duocare.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public partial class NoteItem : ObservableObject
    {
        [ObservableProperty] private string text;
        [ObservableProperty] private bool isDone;
    }

    // ⭐ Inicializamos la colección para evitar null en la navegación
    [ObservableProperty]
    private ObservableCollection<NoteItem> notes = new();

    public IRelayCommand AddNoteCommand { get; }
    public IRelayCommand<NoteItem> ToggleNoteCommand { get; }
    public IRelayCommand<NoteItem> EditNoteCommand { get; }
    public IRelayCommand<NoteItem> DeleteNoteCommand { get; }

    public DashboardViewModel()
    {
        // ⭐ 1. Leer ficha seleccionada
        var ficha = Preferences.Get("FichaSeleccionada", "Ambos");

        // ⭐ 2. Crear lista filtrada según la ficha
        var lista = new List<NoteItem>();

        if (ficha == "Niño" || ficha == "Ambos")
        {
            lista.Add(new NoteItem
            {
                Text = "El niño está tomando antibiótico hasta el jueves.",
                IsDone = false
            });
        }

        if (ficha == "Mascota" || ficha == "Ambos")
        {
            lista.Add(new NoteItem
            {
                Text = "La mascota no puede comer pienso nuevo.",
                IsDone = false
            });
        }

        // ⭐ 3. Rellenar Notes sin reemplazar la colección
        Notes.Clear();
        foreach (var item in lista)
            Notes.Add(item);

        // ⭐ 4. Comandos
        AddNoteCommand = new RelayCommand(OnAddNote);
        ToggleNoteCommand = new RelayCommand<NoteItem>(OnToggleNote);
        EditNoteCommand = new RelayCommand<NoteItem>(OnEditNote);
        DeleteNoteCommand = new RelayCommand<NoteItem>(OnDeleteNote);
    }

    private async void OnAddNote()
    {
        string nuevaNota = await Application.Current.MainPage.DisplayPromptAsync(
            "Nueva nota",
            "Escribe una nota importante:",
            "Guardar",
            "Cancelar",
            placeholder: "Ej: Recordar llevar informe médico"
        );

        if (!string.IsNullOrWhiteSpace(nuevaNota))
            Notes.Add(new NoteItem { Text = nuevaNota, IsDone = false });
    }

    private void OnToggleNote(NoteItem note)
    {
        if (note != null)
            note.IsDone = !note.IsDone;
    }

    private async void OnEditNote(NoteItem note)
    {
        if (note == null) return;

        string editada = await Application.Current.MainPage.DisplayPromptAsync(
            "Editar nota",
            "Modifica el texto:",
            "Guardar",
            "Cancelar",
            initialValue: note.Text
        );

        if (!string.IsNullOrWhiteSpace(editada))
            note.Text = editada;
    }

    private async void OnDeleteNote(NoteItem note)
    {
        if (note == null) return;

        bool confirmar = await Application.Current.MainPage.DisplayAlert(
            "Eliminar nota",
            "¿Seguro que quieres eliminar esta nota?",
            "Sí",
            "No"
        );

        if (confirmar)
            Notes.Remove(note);
    }
}
