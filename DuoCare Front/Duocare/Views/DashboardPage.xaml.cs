using Duocare.ViewModels;

namespace Duocare.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();

        // Animación cuando cambia la colección
        NotesCollection.ChildAdded += NotesCollection_ChildAdded;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Llamar a la API para cargar las notas reales al entrar
        if (BindingContext is DashboardViewModel vm)
        {
            await vm.CargarNotasDesdeApiAsync();
        }
    }

    private async void NotesCollection_ChildAdded(object sender, ElementEventArgs e)
    {
        if (e.Element is not VisualElement element)
            return;

        // Buscar el Frame de la nota
        var frame = element.FindByName<Frame>("AnimatedNote");
        if (frame == null)
            return;

        // Estado inicial
        frame.Opacity = 0;
        frame.Scale = 0.85;

        // Animación suave
        await Task.WhenAll(
            frame.FadeTo(1, 250, Easing.CubicOut),
            frame.ScaleTo(1, 250, Easing.CubicOut)
        );
    }
}