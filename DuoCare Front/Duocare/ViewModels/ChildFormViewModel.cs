using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using Duocare.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Duocare.ViewModels;

public partial class ChildFormViewModel : ObservableObject
{
    [ObservableProperty] private string childName;
    [ObservableProperty] private string age;
    [ObservableProperty] private string allergies;
    [ObservableProperty] private string conditions;
    [ObservableProperty] private string medication;

    public ObservableCollection<string> AgeList { get; } = new()
    {
        "0 años","1 año","2 años","3 años","4 años","5 años",
        "6 años","7 años","8 años","9 años","10 años",
        "11 años","12 años","13 años","14 años","15 años","16 años"
    };

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand AddChildCommand { get; }
    public IRelayCommand RemoveChildCommand { get; }

    private readonly ApiServices _api = new ApiServices();

    public ChildFormViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
        AddChildCommand = new RelayCommand(OnAddChild);
        RemoveChildCommand = new RelayCommand(OnRemoveChild);

        Age = "0 años";
    } // [1](https://stackoverflow.com/questions/74026933/cannot-disable-jwt-claims-mapping-using-jwtsecuritytokenhandler-defaultinboundcl)

    private async void OnSave()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ChildName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Introduce el nombre del niño.", "OK");
                return;
            }

            var medicalData =
                $"Edad: {Age ?? ""}\n" +
                $"Alergias: {Allergies ?? ""}\n" +
                $"Condiciones: {Conditions ?? ""}";

            // ⭐ JSON estructurado
            var extraDataJson = JsonSerializer.Serialize(new
            {
                age = Age ?? "",
                allergies = Allergies ?? "",
                conditions = Conditions ?? ""
            });

            var dto = new RecordCreateRequest(
                Name: ChildName.Trim(),
                Type: "Child",
                Medication: Medication ?? "",
                MedicalData: medicalData,
                Notes: "",
                ExtraDataJson: extraDataJson
            );

            await _api.CreateRecordAsync(dto);

            Preferences.Set("ProfileCompleted", true);

            await Application.Current.MainPage.DisplayAlert("Guardado", "Ficha del niño guardada en el servidor", "OK");

            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//DashboardPage");
            else
            {
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//DashboardPage");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnAddChild()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync(nameof(ChildFormPage));
    } // [1](https://stackoverflow.com/questions/74026933/cannot-disable-jwt-claims-mapping-using-jwtsecuritytokenhandler-defaultinboundcl)

    private async void OnRemoveChild()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("..");
    } // [1](https://stackoverflow.com/questions/74026933/cannot-disable-jwt-claims-mapping-using-jwtsecuritytokenhandler-defaultinboundcl)
}