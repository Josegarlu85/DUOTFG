using Duocare.ViewModels;

namespace Duocare.Views;

public partial class AgreementsPage : ContentPage
{
    public AgreementsPage()
    {
        InitializeComponent();
        BindingContext = new AgreementsViewModel();
    }
}
