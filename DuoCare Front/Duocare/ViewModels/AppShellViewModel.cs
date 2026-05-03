using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using Duocare.Messages;
using System.Threading.Tasks;
using System;

namespace Duocare.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    [ObservableProperty] private string userName;
    [ObservableProperty] private ImageSource userPhoto;

    public IRelayCommand GoToAboutCommand { get; }
    public IRelayCommand GoToSettingsCommand { get; }
    public IRelayCommand LogoutCommand { get; }

    public AppShellViewModel()
    {
        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();

        var rawName = Preferences.Get("UserName", "Usuario");
        UserName = FormatName(rawName);

        string photoPath = "";
        if (!string.IsNullOrWhiteSpace(email))
        {
            photoPath = Preferences.Get($"UserPhoto_{email}", "");
        }

        if (!string.IsNullOrWhiteSpace(photoPath) && File.Exists(photoPath))
            UserPhoto = ImageSource.FromFile(photoPath);
        else
            UserPhoto = "default_avatar.png";

        WeakReferenceMessenger.Default.Register<UserPhotoChangedMessage>(this, (r, m) =>
        {
            if (!string.IsNullOrWhiteSpace(m.Value) && File.Exists(m.Value))
                UserPhoto = ImageSource.FromFile(m.Value);
            else
                UserPhoto = "default_avatar.png";
        });

        WeakReferenceMessenger.Default.Register<UserNameChangedMessage>(this, (r, m) =>
        {
            UserName = FormatName(m.Value);
        });

        GoToAboutCommand = new RelayCommand(OnGoToAbout);
        GoToSettingsCommand = new RelayCommand(OnGoToSettings);
        LogoutCommand = new RelayCommand(OnLogout);
    }

    private string FormatName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Usuario";

        input = input.Replace(".", " ")
                     .Replace("_", " ")
                     .Replace("-", " ");

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();

        return string.Join(" ", words);
    }

    private async void OnGoToAbout()
    {
        await Shell.Current.GoToAsync("//AboutPage");
    }

    private async void OnGoToSettings()
    {
        await Shell.Current.GoToAsync("//DummyPage");
        await Shell.Current.GoToAsync("SettingsPage");
    }

    private async void OnLogout()
    {
        Preferences.Remove("CurrentUserEmail");
        Application.Current.MainPage = new NavigationPage(new LoginPage());
        await Task.CompletedTask;
    }
}