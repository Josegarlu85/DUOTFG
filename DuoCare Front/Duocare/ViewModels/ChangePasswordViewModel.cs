using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using System.Text.RegularExpressions;

namespace Duocare.ViewModels;

public partial class ChangePasswordViewModel : ObservableObject
{
    [ObservableProperty] private string currentPassword;
    [ObservableProperty] private string newPassword;
    [ObservableProperty] private string confirmPassword;

    [ObservableProperty] private bool isCurrentPasswordHidden = true;
    [ObservableProperty] private bool isNewPasswordHidden = true;
    [ObservableProperty] private bool isConfirmPasswordHidden = true;

    [ObservableProperty] private string currentPasswordEmoji = "🙈";
    [ObservableProperty] private string newPasswordEmoji = "🙈";
    [ObservableProperty] private string confirmPasswordEmoji = "🙈";

    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private bool hasError;

    [ObservableProperty] private string passwordStrengthText;
    [ObservableProperty] private Color passwordStrengthColor;
    [ObservableProperty] private bool showStrength;

    private readonly ApiServices _api = new ApiServices();

    public ChangePasswordViewModel()
    {
        HasError = false;
        ShowStrength = false;
    }

    [RelayCommand]
    private void ToggleCurrentPassword()
    {
        IsCurrentPasswordHidden = !IsCurrentPasswordHidden;
        CurrentPasswordEmoji = IsCurrentPasswordHidden ? "🙈" : "👁️";
    }

    [RelayCommand]
    private void ToggleNewPassword()
    {
        IsNewPasswordHidden = !IsNewPasswordHidden;
        NewPasswordEmoji = IsNewPasswordHidden ? "🙈" : "👁️";
    }

    [RelayCommand]
    private void ToggleConfirmPassword()
    {
        IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
        ConfirmPasswordEmoji = IsConfirmPasswordHidden ? "🙈" : "👁️";
    }

    partial void OnNewPasswordChanged(string value)
    {
        EvaluatePasswordStrength(value);
    }

    private void EvaluatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            ShowStrength = false;
            return;
        }

        ShowStrength = true;

        int score = 0;
        if (password.Length >= 6) score++;
        if (password.Length >= 10) score++;
        if (Regex.IsMatch(password, @"[A-Z]")) score++;
        if (Regex.IsMatch(password, @"[0-9]")) score++;
        if (Regex.IsMatch(password, @"[\W_]")) score++;

        switch (score)
        {
            case 1: PasswordStrengthText = "Muy débil"; PasswordStrengthColor = Colors.Red; break;
            case 2: PasswordStrengthText = "Débil"; PasswordStrengthColor = Colors.OrangeRed; break;
            case 3: PasswordStrengthText = "Media"; PasswordStrengthColor = Colors.Orange; break;
            case 4: PasswordStrengthText = "Fuerte"; PasswordStrengthColor = Colors.Green; break;
            case 5: PasswordStrengthText = "Muy fuerte"; PasswordStrengthColor = Colors.DarkGreen; break;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        HasError = false;

        if (string.IsNullOrWhiteSpace(CurrentPassword) ||
            string.IsNullOrWhiteSpace(NewPassword) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ShowError("Completa todos los campos.");
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ShowError("Las contraseñas no coinciden.");
            return;
        }

        if (PasswordStrengthText == "Muy débil" || PasswordStrengthText == "Débil")
        {
            ShowError("La contraseña es demasiado débil.");
            return;
        }

        try
        {
            await _api.ChangePasswordAsync(CurrentPassword, NewPassword);

            await Application.Current.MainPage.DisplayAlert("Éxito", "Tu contraseña ha sido actualizada.", "OK");
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