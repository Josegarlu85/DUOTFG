using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Duocare.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public partial class NoteItem : ObservableObject
    {
        [ObservableProperty] private string text;
        [ObservableProperty] private bool isDone;
    }

    [ObservableProperty]
    private ObservableCollection<NoteItem> notes = new();

    private readonly ApiServices _api = new ApiServices();

    private int? _currentRecordId;

    public IRelayCommand AddNoteCommand { get; }
    public IRelayCommand<NoteItem> ToggleNoteCommand { get; }
    public IRelayCommand<NoteItem> EditNoteCommand { get; }
    public IRelayCommand<NoteItem> DeleteNoteCommand { get; }
    public IAsyncRelayCommand LoadNotesCommand { get; }

    public DashboardViewModel()
    {
        AddNoteCommand = new AsyncRelayCommand(OnAddNote);
        ToggleNoteCommand = new AsyncRelayCommand<NoteItem>(OnToggleNote);
        EditNoteCommand = new AsyncRelayCommand<NoteItem>(OnEditNote);
        DeleteNoteCommand = new AsyncRelayCommand<NoteItem>(OnDeleteNote);
        LoadNotesCommand = new AsyncRelayCommand(CargarNotasDesdeApiAsync);
    }

    public async Task CargarNotasDesdeApiAsync()
    {
        try
        {
            var records = await _api.GetMyRecordsAsync();
            var notasRecord = records.FirstOrDefault(r => r.Name == "Notas del Dashboard");

            if (notasRecord != null)
            {
                _currentRecordId = notasRecord.Id; 

                if (!string.IsNullOrWhiteSpace(notasRecord.ExtraDataJson) && notasRecord.ExtraDataJson != "[]")
                {
                    try
                    {
                        var notasRecuperadas = JsonSerializer.Deserialize<List<NoteItem>>(notasRecord.ExtraDataJson);
                        if (notasRecuperadas != null)
                        {
                            Notes.Clear();
                            foreach (var item in notasRecuperadas)
                                Notes.Add(item);
                        }
                    }
                    catch
                    {
                        Notes.Clear();
                    }
                }
            }
            else
            {
                // Si la BD dice que no hay notas, limpiamos nuestro ID para no enviar basura
                _currentRecordId = null;
                Notes.Clear();
            }
        }
        catch (Exception ex)
        {
            // Ignoramos errores silenciosos de carga inicial para no molestar al usuario
            Console.WriteLine($"Error cargando: {ex.Message}");
        }
    }

    private async Task GuardarEnApiAsync()
    {
        try
        {
            string notasJson = JsonSerializer.Serialize(Notes);

            var recordDto = new RecordCreateRequest(
                Name: "Notas del Dashboard",
                Type: Preferences.Get("FichaSeleccionada", "Ambos") ?? "Ambos",
                Medication: "",
                MedicalData: "",
                Notes: "Lista de tareas guardada desde el Dashboard",
                ExtraDataJson: notasJson
            );

            if (_currentRecordId.HasValue && _currentRecordId.Value > 0)
            {
                try
                {
                    await _api.UpdateRecordAsync(_currentRecordId.Value, recordDto);
                }
                catch
                {
                    var newRecord = await _api.CreateRecordAsync(recordDto);
                    _currentRecordId = newRecord.Id;
                }
            }
            else
            {
                var createdRecord = await _api.CreateRecordAsync(recordDto);
                _currentRecordId = createdRecord.Id;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error crítico: {ex.Message}", "OK");
        }
    }

    private async Task OnAddNote()
    {
        string nuevaNota = await Application.Current.MainPage.DisplayPromptAsync(
            "Nueva nota", "Escribe una nota importante:", "Guardar", "Cancelar");

        if (!string.IsNullOrWhiteSpace(nuevaNota))
        {
            Notes.Add(new NoteItem { Text = nuevaNota, IsDone = false });
            await GuardarEnApiAsync();
        }
    }

    private async Task OnToggleNote(NoteItem note)
    {
        if (note != null)
        {
            note.IsDone = !note.IsDone;
            await GuardarEnApiAsync();
        }
    }

    private async Task OnEditNote(NoteItem note)
    {
        if (note == null) return;
        string editada = await Application.Current.MainPage.DisplayPromptAsync("Editar", "Cambia el texto:", "OK", "Cancelar", initialValue: note.Text);
        if (!string.IsNullOrWhiteSpace(editada) && editada != note.Text)
        {
            note.Text = editada;
            await GuardarEnApiAsync();
        }
    }

    private async Task OnDeleteNote(NoteItem note)
    {
        if (note == null) return;
        if (await Application.Current.MainPage.DisplayAlert("Eliminar", "¿Borrar esta nota?", "Sí", "No"))
        {
            Notes.Remove(note);
            await GuardarEnApiAsync();
        }
    }
}