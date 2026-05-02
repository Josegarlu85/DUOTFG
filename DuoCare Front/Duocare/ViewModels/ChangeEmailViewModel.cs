using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using Microsoft.Maui.Storage;
using System.Text.RegularExpressions;

namespace Duocare.ViewModels;

public partial class ChangeEmailViewModel : ObservableObject
{
    [ObservableProperty] private string currentEmail;
    [ObservableProperty] private string newEmail;
    [ObservableProperty] private string confirmEmail;

    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private bool hasError;

    private readonly ApiServices _api = new ApiServices();

    public ChangeEmailViewModel()
    {
        HasError = false;
        CurrentEmail = Preferences.Get("CurrentUserEmail", "");
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    [RelayCommand]
    private async Task Save()
    {
        HasError = false;

        if (string.IsNullOrWhiteSpace(NewEmail) || string.IsNullOrWhiteSpace(ConfirmEmail))
        {
            ShowError("Completa todos los campos.");
            return;
        }

        if (!IsValidEmail(NewEmail))
        {
            ShowError("El nuevo correo no es válido.");
            return;
        }

        if (NewEmail.Trim().ToLowerInvariant() != ConfirmEmail.Trim().ToLowerInvariant())
        {
            ShowError("Los correos no coinciden.");
            return;
        }

        try
        {
            // ✅ Enviar email de confirmación al nuevo correo
            await _api.RequestEmailChangeAsync(NewEmail.Trim());

            await Application.Current.MainPage.DisplayAlert(
                "Revisa tu correo",
                "Te hemos enviado un enlace al nuevo correo. Ábrelo para confirmar el cambio. Después, inicia sesión con el nuevo correo.",
                "OK"
            );

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