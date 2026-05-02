using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;

namespace Duocare.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    [ObservableProperty]
    private string email;

    public IRelayCommand SendRecoveryEmailCommand { get; }

    // ✅ API
    private readonly ApiServices _api = new ApiServices();

    public ForgotPasswordViewModel()
    {
        SendRecoveryEmailCommand = new RelayCommand(OnSendRecoveryEmail);
    }

    private async void OnSendRecoveryEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Introduce un correo válido", "OK");
            return;
        }

        try
        {
            // ✅ Endpoint real del backend
            await _api.ForgotPasswordAsync(Email.Trim());

            await Application.Current.MainPage.DisplayAlert(
                "Correo enviado",
                $"Si el correo {Email} está registrado, recibirás un enlace de recuperación.",
                "OK"
            );

            if (Shell.Current != null)
                await Shell.Current.GoToAsync("..");
            else
                await Application.Current.MainPage.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"No se pudo conectar con el servidor.\n\nDetalles: {ex.Message}",
                "OK"
            );
        }
    }
}