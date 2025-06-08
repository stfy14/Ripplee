using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ripplee.Models;

namespace Ripplee.Services.Data
{
    public class ChatApiClient
    {
        private readonly HttpClient _httpClient;

        // ИЗМЕНЕНО: Конструктор теперь принимает HttpClient через DI
        public ChatApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CompanionResponse?> FindCompanionAsync(CompanionRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/findCompanion", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CompanionResponse>(responseString);
            }
            catch (Exception)
            {
                // Лучше обрабатывать конкретные исключения, но для начала так сойдет.
                // Возвращаем null или кастомный объект ошибки.
                return null;
            }
        }
    }
}