using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Duocare.Messages;
using Duocare.Services;
using Microsoft.Maui.Storage;

namespace Duocare.ViewModels;

public partial class ChangeNameViewModel : ObservableObject
{
    [ObservableProperty] private string newName;
    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private bool hasError;

    private readonly ApiServices _api = new ApiServices();

    public ChangeNameViewModel()
    {
        HasError = false;
    }

    [RelayCommand]
    private async Task Save()
    {
        HasError = false;

        if (string.IsNullOrWhiteSpace(NewName) || NewName.Trim().Length < 2)
        {
            ErrorMessage = "Introduce un nombre válido.";
            HasError = true;
            return;
        }

        try
        {
            var updated = await _api.ChangeNameAsync(NewName.Trim());

            var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
            Preferences.Set($"UserName_{email}", updated);
            Preferences.Set("UserName", updated);

            WeakReferenceMessenger.Default.Send(new UserNameChangedMessage(updated));

            await Application.Current.MainPage.DisplayAlert("Éxito", "Tu nombre ha sido actualizado.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }
}