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
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Introduce email y contraseña", "OK");
            return;
        }

        try
        {
            var result = await _api.LoginAsync(new LoginRequest(Email.Trim(), Password));

            Preferences.Set("UserName", result.Email);

            var emailKey = result.Email.Trim().ToLowerInvariant();
            var photoPath = Preferences.Get($"UserPhoto_{emailKey}", "");
            if (!string.IsNullOrWhiteSpace(photoPath))
                Preferences.Set("CurrentUserPhotoPath", photoPath);
            else
                Preferences.Remove("CurrentUserPhotoPath");

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
        {
            await Shell.Current.GoToAsync("RegisterPage");
            return;
        }
        await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
    }

    private async void OnForgotPassword()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("ForgotPasswordPage");
            return;
        }

        await Application.Current.MainPage.DisplayAlert("Info", "Página de recuperación no registrada.", "OK");
    }
}