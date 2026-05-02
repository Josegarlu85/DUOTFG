using Duocare.ViewModels;

namespace Duocare.Views;

public partial class ChildEventPage : ContentPage
{
    public ChildEventPage()
    {
        InitializeComponent();
        BindingContext = new ChildEventViewModel();
    }
}
