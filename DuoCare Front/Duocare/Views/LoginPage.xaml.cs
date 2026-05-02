namespace Duocare.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // No usar Shell.Current aquí porque aún no existe
    }

    private void OnThemeButtonClicked(object sender, EventArgs e)
    {
        Application.Current.UserAppTheme =
            Application.Current.UserAppTheme == AppTheme.Light
            ? AppTheme.Dark
            : AppTheme.Light;
    }

    private void OnThemeIconClicked(object sender, EventArgs e)
    {
        Application.Current.UserAppTheme =
            Application.Current.UserAppTheme == AppTheme.Light
            ? AppTheme.Dark
            : AppTheme.Light;
    }

    // ⭐⭐ ESTE ES EL MÉTODO QUE TE FALTABA ⭐⭐
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Aquí normalmente validarías usuario/contraseña

        // ⭐ Activar el Shell
        Application.Current.MainPage = new AppShell();

        // ⭐ Ir al Dashboard dentro del Shell
        await Shell.Current.GoToAsync("//DashboardPage");
    }
}
