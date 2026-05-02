using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duocare.Services;
using Duocare.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Duocare.ViewModels;

public partial class CombinedFormViewModel : ObservableObject
{
    [ObservableProperty] private string childName;
    [ObservableProperty] private string childAge;
    [ObservableProperty] private string childAllergies;
    [ObservableProperty] private string childConditions;
    [ObservableProperty] private string childMedication;

    [ObservableProperty] private string petName;
    [ObservableProperty] private string species;
    [ObservableProperty] private string breed;
    [ObservableProperty] private string petAge;
    [ObservableProperty] private string petAllergies;
    [ObservableProperty] private string petMedication;

    public ObservableCollection<string> AgeList { get; } = new()
    {
        "0 años","1 año","2 años","3 años","4 años","5 años",
        "6 años","7 años","8 años","9 años","10 años",
        "11 años","12 años","13 años","14 años","15 años","16 años"
    };

    public ObservableCollection<string> SpeciesList { get; } = new()
    {
        "Perro","Gato","Conejo","Hámster","Pájaro","Tortuga","Pez","Hurón","Caballo","Otro"
    };

    public ObservableCollection<string> BreedList { get; } = new();

    private readonly Dictionary<string, List<string>> _breedsBySpecies = new()
    {
        { "Perro", new() { "Labrador","Pastor Alemán","Bulldog","Chihuahua","Golden Retriever","Pug","Beagle","Dálmata","Border Collie" } },
        { "Gato", new() { "Persa","Siamés","Maine Coon","Bengalí","Sphynx","British Shorthair","Ragdoll" } },
        { "Conejo", new() { "Enano","Belier","Rex","Gigante de Flandes" } },
        { "Hámster", new() { "Sirio","Ruso","Roborowski","Chino" } },
        { "Pájaro", new() { "Periquito","Canario","Agapornis","Cacatúa" } },
        { "Pez", new() { "Betta","Goldfish","Guppy","Molly","Tetra" } },
        { "Tortuga", new() { "Rusa","Mora","Orejas Rojas","Mediterránea" } },
        { "Hurón", new() { "Standard","Angora" } },
        { "Caballo", new() { "Árabe","Pura Sangre","Andaluz","Frisón" } },
        { "Otro", new() { "Especie no listada" } }
    };

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand AddCombinedCommand { get; }
    public IRelayCommand RemoveCombinedCommand { get; }

    private readonly ApiServices _api = new ApiServices();

    public CombinedFormViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
        AddCombinedCommand = new RelayCommand(OnAddCombined);
        RemoveCombinedCommand = new RelayCommand(OnRemoveCombined);

        ResetForm();
    } // [2](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)

    partial void OnSpeciesChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            LoadBreeds(value);
            Breed = "";
        }
    } // [2](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)

    private void LoadBreeds(string species)
    {
        BreedList.Clear();
        if (_breedsBySpecies.TryGetValue(species, out var list))
            foreach (var b in list) BreedList.Add(b);
    } // [2](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)

    private void ResetForm()
    {
        ChildName = "";
        ChildAge = "0 años";
        ChildAllergies = "";
        ChildConditions = "";
        ChildMedication = "";

        PetName = "";
        Species = "Perro";
        LoadBreeds("Perro");
        Breed = "";
        PetAge = "";
        PetAllergies = "";
        PetMedication = "";
    } // [2](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)

    private async void OnSave()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ChildName) || string.IsNullOrWhiteSpace(PetName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Introduce nombre del niño y de la mascota.", "OK");
                return;
            }

            var childMedicalData =
                $"NIÑO\n" +
                $"Edad: {ChildAge ?? ""}\n" +
                $"Alergias: {ChildAllergies ?? ""}\n" +
                $"Condiciones: {ChildConditions ?? ""}\n";

            var petMedicalData =
                $"MASCOTA\n" +
                $"Especie: {Species ?? ""}\n" +
                $"Raza: {Breed ?? ""}\n" +
                $"Edad: {PetAge ?? ""}\n" +
                $"Alergias: {PetAllergies ?? ""}\n";

            var combinedMedication =
                $"Niño: {ChildMedication ?? ""} | Mascota: {PetMedication ?? ""}";

            // ⭐ JSON estructurado (niño + mascota)
            var extraDataJson = JsonSerializer.Serialize(new
            {
                child = new
                {
                    name = ChildName ?? "",
                    age = ChildAge ?? "",
                    allergies = ChildAllergies ?? "",
                    conditions = ChildConditions ?? "",
                    medication = ChildMedication ?? ""
                },
                pet = new
                {
                    name = PetName ?? "",
                    species = Species ?? "",
                    breed = Breed ?? "",
                    age = PetAge ?? "",
                    allergies = PetAllergies ?? "",
                    medication = PetMedication ?? ""
                }
            });

            var dto = new RecordCreateRequest(
                Name: $"{ChildName.Trim()} + {PetName.Trim()}",
                Type: "Both",
                Medication: combinedMedication,
                MedicalData: childMedicalData + "\n" + petMedicalData,
                Notes: "",
                ExtraDataJson: extraDataJson
            );

            await _api.CreateRecordAsync(dto);

            Preferences.Set("ProfileCompleted", true);

            await Application.Current.MainPage.DisplayAlert("Guardado", "Ficha combinada guardada en el servidor", "OK");

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

    private async void OnAddCombined()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync(nameof(CombinedFormPage));
    } // [2](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)

    private void OnRemoveCombined()
    {
        ResetForm();
    } // [2](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)
}