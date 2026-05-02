using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;

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

    private readonly HttpClient _httpClient;

    private readonly string _apiUrl = "https://localhost:7056/";

    public IRelayCommand AddNoteCommand { get; }
    public IRelayCommand<NoteItem> ToggleNoteCommand { get; }
    public IRelayCommand<NoteItem> EditNoteCommand { get; }
    public IRelayCommand<NoteItem> DeleteNoteCommand { get; }
    public IAsyncRelayCommand LoadNotesCommand { get; }

    public DashboardViewModel()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(_apiUrl) };

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
            var token = Preferences.Get("AuthToken", "");
            if (string.IsNullOrEmpty(token)) return;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/records/me");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("extraDataJson", out JsonElement extraDataElement))
                {
                    var extraData = extraDataElement.GetString();
                    if (!string.IsNullOrEmpty(extraData) && extraData != "[]")
                    {
                        var notasRecuperadas = JsonSerializer.Deserialize<List<NoteItem>>(extraData);
                        if (notasRecuperadas != null)
                        {
                            Notes.Clear();
                            foreach (var item in notasRecuperadas)
                                Notes.Add(item);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar las notas: {ex.Message}", "OK");
        }
    }

    private async Task GuardarEnApiAsync()
    {
        try
        {
            string notasJson = JsonSerializer.Serialize(Notes);

            var recordDto = new
            {
                Name = "Notas del Dashboard",
                Type = Preferences.Get("FichaSeleccionada", "Ambos"),
                Medication = "",
                MedicalData = "",
                Notes = "Lista de tareas guardada desde el Dashboard",
                ExtraDataJson = notasJson
            };

            var token = Preferences.Get("AuthToken", "");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsJsonAsync("api/records", recordDto);

            if (!response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron guardar las notas en la nube.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Fallo de conexión: {ex.Message}", "OK");
        }
    }

    private async Task OnAddNote()
    {
        string nuevaNota = await Application.Current.MainPage.DisplayPromptAsync(
            "Nueva nota", "Escribe una nota importante:", "Guardar", "Cancelar", placeholder: "Ej: Recordar llevar informe médico");

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

        string editada = await Application.Current.MainPage.DisplayPromptAsync(
            "Editar nota", "Modifica el texto:", "Guardar", "Cancelar", initialValue: note.Text);

        if (!string.IsNullOrWhiteSpace(editada) && editada != note.Text)
        {
            note.Text = editada;
            await GuardarEnApiAsync();
        }
    }

    private async Task OnDeleteNote(NoteItem note)
    {
        if (note == null) return;

        bool confirmar = await Application.Current.MainPage.DisplayAlert(
            "Eliminar nota", "¿Seguro que quieres eliminar esta nota?", "Sí", "No");

        if (confirmar)
        {
            Notes.Remove(note);
            await GuardarEnApiAsync();
        }
    }
}