using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Ripplee.Services.Data
{
    // Модель для ответа от сервера
    public class LoginResponse
    {
        public string? Token { get; set; }
    }
    public class ProfileResponse
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? MyGender { get; set; } 
        public string? MyCity { get; set; }   
    }
    public class UserExistsResponse
    {
        public bool Exists { get; set; }
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

        public async Task<bool> CheckUserExistsAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/exists/{username}");
                if (!response.IsSuccessStatusCode)
                {
                    return false; // По умолчанию считаем, что не существует, если есть ошибка
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var existsResponse = JsonSerializer.Deserialize<UserExistsResponse>(responseContent, _serializerOptions);

                return existsResponse?.Exists ?? false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckUserExistsAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var request = new { username, password };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Users/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    // Можно добавить более детальную обработку ошибок
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _serializerOptions);

                return loginResponse?.Token;
            }
            catch (Exception ex)
            {
                // В реальном приложении здесь нужно логировать ошибку
                Debug.WriteLine($"Login failed: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            try
            {
                var request = new { username, password };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/users/register", content);

                // Просто возвращаем, был ли запрос успешным (код 2xx)
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration failed: {ex.Message}");
                return false;
            }
        }

        public async Task<ProfileResponse?> GetProfileAsync()
        {
            try
            {
                // 1. Получаем токен из безопасного хранилища
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("GetProfileAsync: No auth token found.");
                    return null;
                }

                // 2. Устанавливаем заголовок авторизации для этого запроса
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Отправляем GET-запрос
                var response = await _httpClient.GetAsync("api/users/profile");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"GetProfileAsync failed with status code: {response.StatusCode}");
                    return null;
                }

                // 4. Парсим успешный ответ
                var responseContent = await response.Content.ReadAsStringAsync();
                var profileResponse = JsonSerializer.Deserialize<ProfileResponse>(responseContent, _serializerOptions);

                return profileResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetProfileAsync failed: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserCriteriaAsync(string gender, string city)
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return false;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request = new { MyGender = gender, MyCity = city };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/users/criteria", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateUserCriteriaAsync failed: {ex.Message}");
                return false;
            }
        }
    }
}