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

public partial class PetFormViewModel : ObservableObject
{
    [ObservableProperty] private string petName;
    [ObservableProperty] private string species;
    [ObservableProperty] private string breed;
    [ObservableProperty] private string age;
    [ObservableProperty] private string allergies;
    [ObservableProperty] private string medication;

    public ObservableCollection<string> SpeciesList { get; } = new()
    {
        "Perro","Gato","Conejo","Hámster","Pájaro","Tortuga","Pez","Hurón","Caballo","Otro"
    };

    public ObservableCollection<string> Breeds { get; } = new();

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
    public IRelayCommand AddPetCommand { get; }
    public IRelayCommand RemovePetCommand { get; }

    private readonly ApiServices _api = new ApiServices();

    public PetFormViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
        AddPetCommand = new RelayCommand(OnAddPet);
        RemovePetCommand = new RelayCommand(OnRemovePet);

        // Valores iniciales
        Species = "Perro";
        LoadBreeds("Perro");
    } // [1](https://abp.io/community/articles/how-claim-type-works-in-asp-net-core-and-abp-framework-km5dw6g1)

    partial void OnSpeciesChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            LoadBreeds(value);
            Breed = null;
        }
    } // [1](https://abp.io/community/articles/how-claim-type-works-in-asp-net-core-and-abp-framework-km5dw6g1)

    private void LoadBreeds(string species)
    {
        Breeds.Clear();
        if (_breedsBySpecies.TryGetValue(species, out var list))
            foreach (var b in list) Breeds.Add(b);
    } // [1](https://abp.io/community/articles/how-claim-type-works-in-asp-net-core-and-abp-framework-km5dw6g1)

    private async void OnSave()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PetName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Introduce el nombre de la mascota.", "OK");
                return;
            }

            // Texto legible (para mostrar rápido)
            var medicalData =
                $"Especie: {Species ?? ""}\n" +
                $"Raza: {Breed ?? ""}\n" +
                $"Edad: {Age ?? ""}\n" +
                $"Alergias: {Allergies ?? ""}";

            // ⭐ JSON estructurado (para futuro: filtros, estadísticas, etc.)
            var extraDataObj = new
            {
                species = Species ?? "",
                breed = Breed ?? "",
                age = Age ?? "",
                allergies = Allergies ?? ""
            };

            var extraDataJson = JsonSerializer.Serialize(extraDataObj);

            // ✅ DTO ahora incluye ExtraDataJson
            var dto = new RecordCreateRequest(
                Name: PetName.Trim(),
                Type: "Pet",
                Medication: Medication ?? "",
                MedicalData: medicalData,
                Notes: "",
                ExtraDataJson: extraDataJson
            );

            await _api.CreateRecordAsync(dto);

            Preferences.Set("ProfileCompleted", true);

            await Application.Current.MainPage.DisplayAlert("Guardado", "Ficha de mascota guardada en el servidor", "OK");

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

    private void OnAddPet()
    {
        PetName = string.Empty;
        Species = "Perro";
        LoadBreeds("Perro");
        Breed = null;
        Age = string.Empty;
        Allergies = string.Empty;
        Medication = string.Empty;
    } // [1](https://abp.io/community/articles/how-claim-type-works-in-asp-net-core-and-abp-framework-km5dw6g1)

    private async void OnRemovePet()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("..");
    } // [1](https://abp.io/community/articles/how-claim-type-works-in-asp-net-core-and-abp-framework-km5dw6g1)
}
