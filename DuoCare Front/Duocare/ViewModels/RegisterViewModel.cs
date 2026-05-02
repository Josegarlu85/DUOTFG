using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.IO;

namespace Duocare.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private string passwordStatus = string.Empty;
    [ObservableProperty] private string userName = string.Empty;

    private ImageSource _userPhoto = "default_avatar.png";
    public ImageSource UserPhoto
    {
        get => _userPhoto;
        set => SetProperty(ref _userPhoto, value);
    }

    private string? _photoPath;

    public IRelayCommand RegisterCommand { get; }
    public IAsyncRelayCommand PickPhotoCommand { get; }

    // ✅ API
    private readonly ApiServices _api = new ApiServices();

    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(OnRegister);
        PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync);

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Password) ||
                e.PropertyName == nameof(ConfirmPassword))
            {
                ValidatePassword();
            }
        };
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            PasswordStatus = string.Empty;
            return;
        }

        PasswordStatus = Password == ConfirmPassword ? "Coincide" : "No coincide";
    }

    private async Task PickPhotoAsync()
    {
        var photo = await MediaPicker.PickPhotoAsync();
        if (photo == null) return;

        var localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

        using var stream = await photo.OpenReadAsync();
        using var fileStream = File.OpenWrite(localPath);
        await stream.CopyToAsync(fileStream);

        _photoPath = localPath;
        UserPhoto = ImageSource.FromFile(localPath);
    }

    private async void OnRegister()
    {
        if (PasswordStatus != "Coincide" ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(UserName))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Revisa los datos", "Aceptar");
            return;
        }

        try
        {
            // ✅ Registro real en backend
            await _api.RegisterAsync(new RegisterRequest(
                Email.Trim(),
                UserName.Trim(),     // lo enviamos como FullName al backend
                Password,
                ConfirmPassword
            ));

            // (Opcional) guardas localmente la foto para UI
            var emailKey = Email.Trim().ToLowerInvariant();
            Preferences.Set("CurrentUserEmail", emailKey);

            if (!string.IsNullOrEmpty(_photoPath))
            {
                Preferences.Set($"UserPhoto_{emailKey}", _photoPath);
                Preferences.Set("CurrentUserPhotoPath", _photoPath);
            }
            else
            {
                Preferences.Remove("CurrentUserPhotoPath");
            }

            await Application.Current.MainPage.DisplayAlert(
                "OK",
                "Usuario registrado correctamente. Revisa tu correo para confirmar la cuenta antes de iniciar sesión.",
                "Aceptar"
            );

            // ✅ vuelve atrás a Login
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }
}