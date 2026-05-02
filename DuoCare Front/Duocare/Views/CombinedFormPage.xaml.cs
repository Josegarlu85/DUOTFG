namespace Duocare.Views;

public partial class CombinedFormPage : ContentPage
{
    public CombinedFormPage()
    {
        InitializeComponent();
    }

    private void OnBreedChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;

        if (picker?.SelectedItem is string value)
        {
            if (value.StartsWith("—"))
            {
                picker.SelectedIndex = -1;
            }
        }
    }
}
