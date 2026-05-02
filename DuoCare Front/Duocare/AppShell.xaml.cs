using Duocare.ViewModels;
using Duocare.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Duocare;

public partial class AppShell : Shell
{
    private bool _redirectDone;

    public AppShell()
    {
        InitializeComponent();
        BindingContext = new AppShellViewModel();

        // =========================
        // RUTAS PRINCIPALES
        // =========================
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ProfileSetupPage), typeof(ProfileSetupPage));
        Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));

        // =========================
        // FORMULARIOS
        // =========================
        Routing.RegisterRoute("ChildFormPage", typeof(ChildFormPage));
        Routing.RegisterRoute("PetFormPage", typeof(PetFormPage));
        Routing.RegisterRoute("CombinedFormPage", typeof(CombinedFormPage));

        // =========================
        // EVENTOS
        // =========================
        Routing.RegisterRoute("EventDetailsPage", typeof(EventDetailsPage));
        Routing.RegisterRoute("ChildEventPage", typeof(ChildEventPage));
        Routing.RegisterRoute("PetEventPage", typeof(PetEventPage));
        Routing.RegisterRoute("CalendarEventPage", typeof(CalendarEventPage));

        // =========================
        // OTRAS PÁGINAS
        // =========================
        Routing.RegisterRoute("AgreementsPage", typeof(AgreementsPage));
        Routing.RegisterRoute("CitasPage", typeof(CitasPage));
        Routing.RegisterRoute("AboutPage", typeof(AboutPage));

        // =========================
        // AJUSTES DE CUENTA
        // =========================
        Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        Routing.RegisterRoute(nameof(ChangeEmailPage), typeof(ChangeEmailPage));
        Routing.RegisterRoute(nameof(ChangeNamePage), typeof(ChangeNamePage));
        Routing.RegisterRoute(nameof(ChangePhotoPage), typeof(ChangePhotoPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

        // ✅ Redirección inicial segura
        Navigated += OnFirstNavigated;
    }

    // =========================
    // REDIRECT INICIAL (una sola vez)
    // =========================
    private void OnFirstNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        if (_redirectDone)
            return;

        _redirectDone = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RedirectAsync();
        });
    }

    // =========================
    // LÓGICA DE REDIRECCIÓN JWT
    // =========================
    private async Task RedirectAsync()
    {
        //  1. Comprobar JWT (fuente de verdad)
        var token = Preferences.Get("AuthToken", string.Empty);

        if (string.IsNullOrWhiteSpace(token))
        {
            //  No autenticado → Login
            await GoToAsync("//LoginPage");
            return;
        }

        //  2. Usuario autenticado → comprobar perfil
        var profileCompleted = Preferences.Get("ProfileCompleted", false);

        if (!profileCompleted)
        {
            // Perfil pendiente → ProfileSetup
            await GoToAsync("ProfileSetupPage");
        }
        else
        {
            //  Todo OK → Dashboard
            await GoToAsync("//DashboardPage");
        }
    }
}