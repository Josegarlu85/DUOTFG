using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Views;

namespace Duocare.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool notificationsEnabled;

    public SettingsViewModel()
    {
        // ⭐ Cargar notificaciones del usuario actual
        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
        var notifKey = $"UserNotifications_{email}";

        NotificationsEnabled = Preferences.Get(notifKey, true);
    }

    partial void OnNotificationsEnabledChanged(bool value)
    {
        // ⭐ Guardar notificaciones por usuario
        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
        var notifKey = $"UserNotifications_{email}";

        Preferences.Set(notifKey, value);
    }

    // ⭐ Navegación CORRECTA con rutas absolutas
    [RelayCommand]
    private async Task ChangePassword()
        => await Shell.Current.GoToAsync("//SettingsPage/ChangePasswordPage");

    [RelayCommand]
    private async Task ChangeName()
        => await Shell.Current.GoToAsync("//SettingsPage/ChangeNamePage");

    [RelayCommand]
    private async Task ChangeProfilePhoto()
        => await Shell.Current.GoToAsync("//SettingsPage/ChangePhotoPage");
}
