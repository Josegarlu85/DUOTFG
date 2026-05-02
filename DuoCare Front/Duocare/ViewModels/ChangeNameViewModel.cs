using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Duocare.Messages;
using Duocare.Services;
using Microsoft.Maui.Storage;

namespace Duocare.ViewModels;

public partial class ChangeNameViewModel : ObservableObject
{
    [ObservableProperty] private string currentName;
    [ObservableProperty] private string newName;

    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private bool hasError;

    private readonly ApiServices _api = new ApiServices();

    public ChangeNameViewModel()
    {
        HasError = false;

        // Solo UI: nombre actual (si guardas el email en prefs)
        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
        var nameKey = $"UserName_{email}";
        CurrentName = Preferences.Get(nameKey, "");
    }

    [RelayCommand]
    private async Task Save()
    {
        HasError = false;

        if (string.IsNullOrWhiteSpace(NewName) || NewName.Trim().Length < 2)
        {
            ShowError("Introduce un nombre válido.");
            return;
        }

        try
        {
            // ✅ backend real
            var updated = await _api.ChangeNameAsync(NewName.Trim());

            // cache UI local (opcional)
            var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
            Preferences.Set($"UserName_{email}", updated);
            Preferences.Set("UserName", updated);

            WeakReferenceMessenger.Default.Send(new UserNameChangedMessage(updated));

            await Application.Current.MainPage.DisplayAlert("Éxito", "Tu nombre ha sido actualizado.", "OK");
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }
}