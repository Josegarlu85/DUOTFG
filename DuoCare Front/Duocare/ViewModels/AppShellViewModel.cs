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

    // Declaración de todos los comandos necesarios
    public IRelayCommand GoToDashboardCommand { get; }
    public IRelayCommand GoToAboutCommand { get; }
    public IRelayCommand GoToSettingsCommand { get; }
    public IRelayCommand LogoutCommand { get; }

    public AppShellViewModel()
    {
        var email = Preferences.Get("CurrentUserEmail", "").Trim().ToLowerInvariant();
        string nameKey = string.IsNullOrWhiteSpace(email) ? "UserName" : $"UserName_{email}";
        string rawName = Preferences.Get(nameKey, Preferences.Get("UserName", "Usuario"));

        if (string.IsNullOrWhiteSpace(rawName) || rawName == "Usuario")
        {
            UserName = !string.IsNullOrWhiteSpace(email) ? email : "Usuario";
        }
        else
        {
            UserName = FormatName(rawName);
        }

        string photoPath = !string.IsNullOrWhiteSpace(email) ? Preferences.Get($"UserPhoto_{email}", "") : "";
        if (!string.IsNullOrWhiteSpace(photoPath) && File.Exists(photoPath))
            UserPhoto = ImageSource.FromFile(photoPath);
        else
            UserPhoto = "default_avatar.png";

        GoToDashboardCommand = new RelayCommand(OnGoToDashboard);
        GoToAboutCommand = new RelayCommand(OnGoToAbout);
        GoToSettingsCommand = new RelayCommand(OnGoToSettings);
        LogoutCommand = new RelayCommand(OnLogout);

        WeakReferenceMessenger.Default.Register<UserPhotoChangedMessage>(this, (r, m) =>
        {
            UserPhoto = (!string.IsNullOrWhiteSpace(m.Value) && File.Exists(m.Value))
                ? ImageSource.FromFile(m.Value)
                : "default_avatar.png";
        });

        WeakReferenceMessenger.Default.Register<UserNameChangedMessage>(this, (r, m) =>
        {
            UserName = string.IsNullOrWhiteSpace(m.Value) ? email : FormatName(m.Value);
        });
    }

    private async void OnGoToDashboard()
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private async void OnGoToAbout() => await Shell.Current.GoToAsync("//AboutPage");

    private async void OnGoToSettings()
    {
        await Shell.Current.GoToAsync("//DummyPage");
        await Shell.Current.GoToAsync("SettingsPage");
    }

    private async void OnLogout()
    {
        Preferences.Remove("CurrentUserEmail");
        Preferences.Remove("AuthToken");
        Application.Current.MainPage = new NavigationPage(new LoginPage());
        await Task.CompletedTask;
    }

    private string FormatName(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Contains("@")) return input;

        input = input.Replace(".", " ").Replace("_", " ").Replace("-", " ");
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }

        return string.Join(" ", words);
    }
}