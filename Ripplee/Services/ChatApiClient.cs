using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ripplee.Services.Data
{
    internal class ErrorResponseMessageDto
    {
        public string? Message { get; set; }
    }

    internal class UploadAvatarResponseDto
    {
        public string? AvatarUrl { get; set; }
    }

    internal class ChangeUsernameResponseDto
    {
        public string? Token { get; set; }
        public string? Message { get; set; }
    }
}

namespace Ripplee.Services.Data
{
    public class ProfileResponse // public, так как используется в UserService
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? MyGender { get; set; }
        public string? MyCity { get; set; }
        public string? AvatarUrl { get; set; } 
    }
    public class UserExistsResponse 
    {
        public bool Exists { get; set; }
    }
    public class LoginResponse 
    {
        public string? Token { get; set; }
    }


    public class ChatApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;

        public ChatApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task<bool> IsConnectedToInternet()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        public async Task<bool> CheckUserExistsAsync(string username)
        {
            if (!await IsConnectedToInternet()) { Debug.WriteLine("CheckUserExistsAsync: No internet."); return false; }
            try
            {
                var response = await _httpClient.GetAsync($"api/users/exists/{Uri.EscapeDataString(username)}");
                if (!response.IsSuccessStatusCode) return false;
                var responseContent = await response.Content.ReadAsStringAsync();
                var existsResponse = JsonSerializer.Deserialize<UserExistsResponse>(responseContent, _serializerOptions);
                return existsResponse?.Exists ?? false;
            }
            catch (Exception ex) { Debug.WriteLine($"CheckUserExistsAsync failed: {ex.Message}"); return false; }
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            if (!await IsConnectedToInternet()) { Debug.WriteLine("LoginAsync: No internet."); return null; }
            try
            {
                var request = new { username, password };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/Users/login", content);
                if (!response.IsSuccessStatusCode) return null;
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _serializerOptions);
                return loginResponse?.Token;
            }
            catch (Exception ex) { Debug.WriteLine($"Login failed: {ex.Message}"); return null; }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (!await IsConnectedToInternet()) { Debug.WriteLine("RegisterAsync: No internet."); return false; }
            try
            {
                var request = new { username, password };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/register", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Debug.WriteLine($"Registration failed: {ex.Message}"); return false; }
        }

        public async Task<ProfileResponse?> GetProfileAsync()
        {
            if (!await IsConnectedToInternet()) { Debug.WriteLine("GetProfileAsync: No internet."); return null; }
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return null;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/users/profile");
                _httpClient.DefaultRequestHeaders.Authorization = null; 
                if (!response.IsSuccessStatusCode) return null;
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProfileResponse>(responseContent, _serializerOptions);
            }
            catch (Exception ex) { Debug.WriteLine($"GetProfileAsync failed: {ex.Message}"); return null; }
        }

        public async Task<(bool Success, string? NewToken)> UpdateUserCriteriaAsync(string gender, string city)
        {
            if (!await IsConnectedToInternet()) { Debug.WriteLine("UpdateUserCriteriaAsync: No internet."); return (false, null); }
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return (false, null);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = new { MyGender = gender, MyCity = city };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/criteria", content);
                _httpClient.DefaultRequestHeaders.Authorization = null;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ChangeUsernameResponseDto>(responseContent, _serializerOptions); 
                    return (true, result?.Token);
                }
                return (false, null);
            }
            catch (Exception ex) { Debug.WriteLine($"UpdateUserCriteriaAsync failed: {ex.Message}"); return (false, null); }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordOnServerAsync(string oldPassword, string newPassword)
        {
            if (!await IsConnectedToInternet()) return (false, "No internet connection.");
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return (false, "Not authenticated.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = new { oldPassword, newPassword };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/change-password", content);
                _httpClient.DefaultRequestHeaders.Authorization = null;

                if (response.IsSuccessStatusCode) return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ErrorResponseMessageDto>(errorContent, _serializerOptions);
                    return (false, errorObj?.Message ?? "Unknown server error.");
                }
                catch { return (false, $"Server error ({(int)response.StatusCode})."); }
            }
            catch (Exception ex) { Debug.WriteLine($"ChangePasswordOnServerAsync exception: {ex.Message}"); return (false, "An unexpected error occurred."); }
        }

        private string GetMimeTypeForMaui(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch { ".jpg" => "image/jpeg", ".jpeg" => "image/jpeg", ".png" => "image/png", _ => "application/octet-stream" };
        }

        public async Task<string?> UploadAvatarToServerAsync(Stream imageData, string fileName)
        {
            if (!await IsConnectedToInternet()) return null;
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return null;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(imageData);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeTypeForMaui(fileName));
                content.Add(streamContent, "file", fileName);
                var response = await _httpClient.PostAsync("api/users/avatar", content);
                _httpClient.DefaultRequestHeaders.Authorization = null;

                if (!response.IsSuccessStatusCode) return null;

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UploadAvatarResponseDto>(responseContent, _serializerOptions);
                return result?.AvatarUrl;
            }
            catch (Exception ex) { Debug.WriteLine($"UploadAvatarToServerAsync exception: {ex.Message}"); return null; }
        }

        public async Task<(string? NewToken, string? ErrorMessage)> ChangeUsernameOnServerAsync(string newUsername, string currentPassword)
        {
            if (!await IsConnectedToInternet()) return (null, "No internet connection.");
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return (null, "Not authenticated.");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = new { newUsername, currentPassword };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/change-username", content);
                _httpClient.DefaultRequestHeaders.Authorization = null;

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<ErrorResponseMessageDto>(errorBody, _serializerOptions);
                        return (null, errorObj?.Message ?? "Unknown server error.");
                    }
                    catch { return (null, $"Server error ({(int)response.StatusCode})."); }
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChangeUsernameResponseDto>(responseContent, _serializerOptions);
                return (result?.Token, result?.Message);
            }
            catch (Exception ex) { Debug.WriteLine($"ChangeUsernameOnServerAsync exception: {ex.Message}"); return (null, "An unexpected error occurred."); }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAccountOnServerAsync(string password)
        {
            if (!await IsConnectedToInternet()) return (false, "No internet connection.");
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return (false, "Not authenticated.");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request = new { password };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/delete-account", content); 

                _httpClient.DefaultRequestHeaders.Authorization = null;

                if (response.IsSuccessStatusCode) return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ErrorResponseMessageDto>(errorContent, _serializerOptions);
                    return (false, errorObj?.Message ?? "Unknown server error.");
                }
                catch { return (false, $"Server error ({(int)response.StatusCode})."); }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteAccountOnServerAsync exception: {ex.Message}");
                return (false, "An unexpected error occurred.");
            }
        }
    }
}