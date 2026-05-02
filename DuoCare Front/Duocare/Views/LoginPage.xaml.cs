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
}