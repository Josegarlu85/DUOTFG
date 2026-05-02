using Duocare.Services;

namespace Duocare
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            var api = new ApiServices();
            var result = await api.GetTestAsync();

            await DisplayAlert("Respuesta API", result, "OK");
        }
    }
}
