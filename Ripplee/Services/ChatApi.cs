using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ripplee.Models;
// скорее всего придётся отказаться от этого метода (НО ПОКА ПУСТЬ БУДЕТ)

namespace Ripplee.Services.Data
{
    public class ChatApiClient
    {
        private readonly HttpClient _httpClient;

        public ChatApiClient(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<CompanionResponse> FindCompanionAsync(CompanionRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/findCompanion", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                //перестать возвращать в json  
                return JsonConvert.DeserializeObject<CompanionResponse>(responseString);
            }
            catch (HttpRequestException e)
            {
                return new CompanionResponse
                {
                    Success = false,
                    Message = $"Ошибка при запросе к серверу: {e.Message}"
                };
            }
            catch (Exception e)
            {
                return new CompanionResponse
                {
                    Success = false,
                    Message = $"Неизвестная ошибка: {e.Message}"
                };
            }
        }
    }
}