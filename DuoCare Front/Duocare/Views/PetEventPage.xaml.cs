using Duocare.ViewModels;

namespace Duocare.Views;

public partial class PetEventPage : ContentPage
{
    public PetEventPage()
    {
        InitializeComponent();
        BindingContext = new PetEventViewModel();
    }
}
