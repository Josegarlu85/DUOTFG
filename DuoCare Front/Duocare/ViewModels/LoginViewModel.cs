using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using Duocare.Views;
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
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Introduce email y contraseña", "OK");
            return;
        }

        try
        {
            var result = await _api.LoginAsync(new LoginRequest(Email.Trim(), Password));
            var emailKey = result.Email.Trim().ToLowerInvariant();

            Preferences.Set("CurrentUserEmail", emailKey);
            Preferences.Set("UserName", result.FullName);
            Preferences.Set($"UserName_{emailKey}", result.FullName);

            // === LOGICA DE PRIMER LOGIN POR USUARIO ===
            // Comprobamos si este email específico ya ha entrado alguna vez
            bool yaHaEntradoAntes = Preferences.Get($"UserHasLoggedIn_{emailKey}", false);

            if (!yaHaEntradoAntes)
            {
                // Es su primera vez: forzamos que salga la configuración de perfil
                Preferences.Set("ProfileCompleted", false);
                Preferences.Set($"UserHasLoggedIn_{emailKey}", true); // Marcamos que para la prpxima ya no es nuevo
            }
            else
            {
                Preferences.Set("ProfileCompleted", true);
            }

            if (!string.IsNullOrWhiteSpace(result.ProfilePhotoBase64))
            {
                var photoPath = Path.Combine(FileSystem.AppDataDirectory, $"{emailKey}_profile.jpg");
                var bytes = Convert.FromBase64String(result.ProfilePhotoBase64);
                File.WriteAllBytes(photoPath, bytes);
                Preferences.Set($"UserPhoto_{emailKey}", photoPath);
                Preferences.Set("CurrentUserPhotoPath", photoPath);
            }

            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnRegister()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("RegisterPage");
        else
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
    }

    private async void OnForgotPassword()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("ForgotPasswordPage");
        else
            await Application.Current.MainPage.DisplayAlert("Info", "Página de recuperación no registrada.", "OK");
    }
}