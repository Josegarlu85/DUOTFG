using Microsoft.Maui.Controls;

namespace Duocare.Views;

public partial class ChildFormPage : ContentPage
{
    public ChildFormPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = true,
            Command = new Command(async () =>
            {
                await Shell.Current.GoToAsync("..");
            })
        });
    }
}
