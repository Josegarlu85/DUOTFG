namespace Duocare.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
    }
}
