namespace Duocare.Services;

using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class ApiServices
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5032/")
    };

    // ============================================================
    // Helpers JWT
    // ============================================================
    public void SetBearerToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
    }

    private void EnsureBearerFromPreferences()
    {
        if (_httpClient.DefaultRequestHeaders.Authorization != null)
            return;

        var token = Preferences.Get("AuthToken", "");
        if (!string.IsNullOrWhiteSpace(token))
            SetBearerToken(token);
    }

    public void ClearSession()
    {
        Preferences.Remove("AuthToken");
        Preferences.Remove("CurrentUserId");
        Preferences.Remove("CurrentUserEmail");
        Preferences.Remove("CurrentUserPhotoPath");
        Preferences.Remove("UserName");
        Preferences.Set("ProfileCompleted", false);
        SetBearerToken(null);
    }

    // ============================================================
    // TEST
    // ============================================================
    public async Task<string> GetTestAsync()
        => await _httpClient.GetStringAsync("api/test");

    // ============================================================
    // AUTH
    // ============================================================
    public async Task RegisterAsync(RegisterRequest dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        var data = await response.Content.ReadFromJsonAsync<LoginResponse>()
                   ?? throw new Exception("Respuesta de login vacía.");

        Preferences.Set("AuthToken", data.Token);
        Preferences.Set("CurrentUserId", data.UserId);
        Preferences.Set("CurrentUserEmail", data.Email);
        SetBearerToken(data.Token);

        return data;
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { Email = email });
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);
    }

    public async Task LogoutAsync()
    {
        EnsureBearerFromPreferences();
        _ = await _httpClient.PostAsync("api/auth/logout", content: null);
        ClearSession();
    }

    // ============================================================
    // AUTH - Change*
    // ============================================================
    public async Task RequestEmailChangeAsync(string newEmail)
    {
        EnsureBearerFromPreferences();

        var res = await _httpClient.PostAsJsonAsync(
            "api/auth/request-email-change",
            new RequestEmailChangeRequest(newEmail)
        );

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(body);
    }

    public async Task<string> ChangeNameAsync(string newName)
    {
        EnsureBearerFromPreferences();

        var res = await _httpClient.PostAsJsonAsync(
            "api/auth/change-name",
            new ChangeNameRequest(newName)
        );

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(body);

        var json = await res.Content.ReadFromJsonAsync<ChangeNameResponse>();
        return json?.FullName ?? newName;
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        EnsureBearerFromPreferences();

        var res = await _httpClient.PostAsJsonAsync(
            "api/auth/change-password",
            new ChangePasswordRequest(currentPassword, newPassword)
        );

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(body);
    }

    public async Task ChangePhotoAsync(string base64Image)
    {
        EnsureBearerFromPreferences();

        var res = await _httpClient.PostAsJsonAsync(
            "api/auth/change-photo",
            new ChangePhotoRequest(base64Image)
        );

        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(body);
    }

    // ============================================================
    // USERS (PROTEGIDO) -> UsersController
    // ============================================================
    public async Task<UserListDto> FindUserByEmailAsync(string email)
    {
        EnsureBearerFromPreferences();

        var url = $"api/users/find?email={Uri.EscapeDataString(email)}";
        var response = await _httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<UserListDto>()
               ?? throw new Exception("Respuesta vacía al buscar usuario.");
    }

    // ============================================================
    // RECORDS (PROTEGIDO) -> RecordsController
    // ============================================================
    public async Task<RecordResponse> CreateRecordAsync(RecordCreateRequest dto)
    {
        EnsureBearerFromPreferences();

        var response = await _httpClient.PostAsJsonAsync("api/records", dto);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<RecordResponse>()
               ?? throw new Exception("Respuesta vacía al crear registro.");
    }

    public async Task<RecordResponse?> GetMyRecordAsync()
    {
        EnsureBearerFromPreferences();

        var response = await _httpClient.GetAsync("api/records/me");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<RecordResponse?>();
    }

    public async Task<List<RecordResponse>> GetMyRecordsAsync()
    {
        EnsureBearerFromPreferences();

        var response = await _httpClient.GetAsync("api/records/me/all");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<List<RecordResponse>>()
               ?? new List<RecordResponse>();
    }

    public async Task<RecordResponse?> GetRecordsByUserIdAsync(string userId)
    {
        EnsureBearerFromPreferences();

        var response = await _httpClient.GetAsync($"api/records/user/{Uri.EscapeDataString(userId)}");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<RecordResponse?>();
    }

    // ============================================================
    // ✅ APPOINTMENTS (PROTEGIDO) -> AppointmentsController
    // ============================================================
    public async Task<AppointmentResponse> CreateAppointmentAsync(AppointmentCreateRequest dto)
    {
        EnsureBearerFromPreferences();

        var response = await _httpClient.PostAsJsonAsync("api/appointments", dto);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<AppointmentResponse>()
               ?? throw new Exception("Respuesta vacía al crear cita.");
    }

    public async Task<AppointmentsPagedResponse> GetMyAppointmentsAsync(int page = 1, int pageSize = 10)
    {
        EnsureBearerFromPreferences();

        var response = await _httpClient.GetAsync($"api/appointments/me?page={page}&pageSize={pageSize}");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<AppointmentsPagedResponse>()
               ?? new AppointmentsPagedResponse();
    }

    public async Task<AppointmentResponse?> AcceptAppointmentAsync(int id)
    {
        EnsureBearerFromPreferences();

        using var content = new StringContent("");
        var response = await _httpClient.PutAsync($"api/appointments/{id}/accept", content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<AppointmentResponse?>();
    }

    public async Task<AppointmentResponse?> RejectAppointmentAsync(int id)
    {
        EnsureBearerFromPreferences();

        using var content = new StringContent("");
        var response = await _httpClient.PutAsync($"api/appointments/{id}/reject", content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<AppointmentResponse?>();
    }

    public async Task<AppointmentResponse?> CompleteAppointmentAsync(int id)
    {
        EnsureBearerFromPreferences();

        using var content = new StringContent("");
        var response = await _httpClient.PutAsync($"api/appointments/{id}/complete", content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new Exception(body);

        return await response.Content.ReadFromJsonAsync<AppointmentResponse?>();
    }
}

// ============================================================
// DTOs FRONTEND
// ============================================================

public record RegisterRequest(string Email, string FullName, string Password, string ConfirmPassword);
public record LoginRequest(string Email, string Password);

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime Expires { get; set; }
}

public class UserListDto
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
}

public record RecordCreateRequest(string Name, string Type, string Medication, string MedicalData, string Notes, string ExtraDataJson);

public class RecordResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Medication { get; set; } = "";
    public string MedicalData { get; set; } = "";
    public string Notes { get; set; } = "";
    public string UserId { get; set; } = "";
    public string ExtraDataJson { get; set; } = "";
}

public record RequestEmailChangeRequest(string NewEmail);
public record ChangeNameRequest(string NewName);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ChangePhotoRequest(string Base64Image);

public class ChangeNameResponse
{
    public string FullName { get; set; } = "";
}

// Appointments DTOs (Frontend)
public record AppointmentCreateRequest(string ReceiverId, DateTime Date, double Latitude, double Longitude);

public class AppointmentResponse
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SenderId { get; set; } = "";
    public string ReceiverId { get; set; } = "";
    public string Status { get; set; } = "";
    public string CreatedBy { get; set; } = "";
}

public class AppointmentsPagedResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<AppointmentResponse> Data { get; set; } = new();
}