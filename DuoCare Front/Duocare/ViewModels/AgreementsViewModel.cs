using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;

namespace Duocare.ViewModels;

public partial class AgreementsViewModel : ObservableObject
{
    [ObservableProperty]
    private string pdfSource;

    public AgreementsViewModel()
    {
        // El PDF se generará y se guardará en local
        GenerateAndLoadPdf();
    }

    private void GenerateAndLoadPdf()
    {
        var bytes = PdfService.GenerateAgreementsPdf();
        var file = Path.Combine(FileSystem.CacheDirectory, "acuerdos.pdf");
        File.WriteAllBytes(file, bytes);

        PdfSource = file;
    }

    [RelayCommand]
    public async Task DownloadPdf()
    {
        var file = Path.Combine(FileSystem.CacheDirectory, "acuerdos.pdf");

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Acuerdos",
            File = new ShareFile(file)
        });
    }
}
