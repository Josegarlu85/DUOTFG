using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;

namespace Duocare.ViewModels;

public partial class UserSearchViewModel : ObservableObject
{
    private string _emailToSearch = "";
    public string EmailToSearch
    {
        get => _emailToSearch;
        set => SetProperty(ref _emailToSearch, value);
    }

    private string _resultText = "";
    public string ResultText
    {
        get => _resultText;
        set => SetProperty(ref _resultText, value);
    }

    private readonly ApiServices _api = new ApiServices();

    public IAsyncRelayCommand SearchCommand { get; }

    public UserSearchViewModel()
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailToSearch))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Introduce un email", "OK");
            return;
        }

        try
        {
            var user = await _api.FindUserByEmailAsync(EmailToSearch.Trim());
            ResultText = $"Encontrado:\n{user.FullName}\n{user.Email}\nId: {user.Id}";
        }
        catch (Exception ex)
        {
            ResultText = "";
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}