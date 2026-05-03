using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Duocare.Messages;
using Duocare.Services;
using Microsoft.Maui.Storage;
using System.IO;

namespace Duocare.ViewModels;

public partial class ChangePhotoViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreview))]
    private string previewImage;

    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private bool hasError;

    private FileResult _selectedPhoto;

    private readonly ApiServices _api = new ApiServices();

    public ChangePhotoViewModel()
    {
        HasError = false;

        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
        var photoKey = $"UserPhoto_{email}";
        PreviewImage = Preferences.Get(photoKey, "");
    }

    public bool HasPreview => !string.IsNullOrWhiteSpace(PreviewImage);

    [RelayCommand]
    private async Task PickFromGallery()
    {
        try
        {
            _selectedPhoto = await MediaPicker.PickPhotoAsync();
            if (_selectedPhoto != null)
                PreviewImage = _selectedPhoto.FullPath;
        }
        catch
        {
            ShowError("No se pudo abrir la galería.");
        }
    }

    [RelayCommand]
    private async Task TakePhoto()
    {
        try
        {
            _selectedPhoto = await MediaPicker.CapturePhotoAsync();
            if (_selectedPhoto != null)
                PreviewImage = _selectedPhoto.FullPath;
        }
        catch
        {
            ShowError("No se pudo acceder a la cámara.");
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_selectedPhoto == null && string.IsNullOrWhiteSpace(PreviewImage))
        {
            ShowError("Selecciona o toma una foto primero.");
            return;
        }

        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError("No se encontró el usuario actual.");
            return;
        }

        try
        {
            string destinationPath = PreviewImage;

            if (_selectedPhoto != null)
            {
                var fileName = $"{email}_profile.jpg";
                destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                File.Copy(PreviewImage, destinationPath, overwrite: true);
            }

            var photoKey = $"UserPhoto_{email}";
            Preferences.Set(photoKey, destinationPath);

            if (_selectedPhoto != null)
            {
                var bytes = File.ReadAllBytes(destinationPath);
                var base64 = Convert.ToBase64String(bytes);
                await _api.ChangePhotoAsync(base64);
            }

            WeakReferenceMessenger.Default.Send(new UserPhotoChangedMessage(destinationPath));

            await Application.Current.MainPage.DisplayAlert("Éxito", "Tu foto de perfil ha sido actualizada.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            ShowError("Error al guardar la foto.");
        }
    }

    private void ShowError(string msg)
    {
        ErrorMessage = msg;
        HasError = true;
    }
}