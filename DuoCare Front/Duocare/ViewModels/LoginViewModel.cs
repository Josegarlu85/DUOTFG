using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using Duocare.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Duocare.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public IRelayCommand LoginCommand { get; }
    public IRelayCommand RegisterCommand { get; }
    public IRelayCommand ForgotPasswordCommand { get; }

    private readonly ApiServices _api = new ApiServices();

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(OnLogin);
        RegisterCommand = new RelayCommand(OnRegister);
        ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
    }

    private async void OnLogin()
    {
        // Validación rápida
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Introduce email y contraseña", "OK");
            return;
        }

        try
        {
            // ✅ Login real contra el backend (devuelve token, userId, email, expires)
            // ✅ ApiServices.LoginAsync ya guarda AuthToken/CurrentUserId/CurrentUserEmail
            // ✅ y aplica Authorization: Bearer automáticamente
            var result = await _api.LoginAsync(new LoginRequest(Email.Trim(), Password));

            // ✅ Nombre visible en el menú (por ahora usamos el email)
            Preferences.Set("UserName", result.Email);

            // ✅ Foto por usuario (si existe)
            var emailKey = result.Email.Trim().ToLowerInvariant();
            var photoPath = Preferences.Get($"UserPhoto_{emailKey}", "");
            if (!string.IsNullOrWhiteSpace(photoPath))
                Preferences.Set("CurrentUserPhotoPath", photoPath);
            else
                Preferences.Remove("CurrentUserPhotoPath");

            // ✅ Crear Shell como raíz
            Application.Current.MainPage = new AppShell();

            // ✅ Navegar cuando el Shell ya esté listo
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("//DashboardPage");
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnRegister()
    {
        // ✅ Si hay Shell, navega por rutas
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("RegisterPage");
            return;
        }

        // ✅ Si NO hay Shell, usa Navigation normal
        await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
    }

    private async void OnForgotPassword()
    {
        // ✅ Si tienes una ruta en AppShell (por ejemplo "ForgotPasswordPage"), navega a ella
        if (Shell.Current != null)
        {
            // Cambia "ForgotPasswordPage" por la ruta real si la has registrado con Routing.RegisterRoute(...)
            await Shell.Current.GoToAsync("ForgotPasswordPage");
            return;
        }

        // ✅ Fallback si aún no tienes ruta/página
        await Application.Current.MainPage.DisplayAlert(
            "Info",
            "Crea y registra la ruta/página ForgotPasswordPage para navegar aquí.",
            "OK");
    }
}