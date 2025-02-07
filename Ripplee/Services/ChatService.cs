using System.Threading.Tasks;
using Ripplee.Services.Data;
using Ripplee.Models;

namespace Ripplee.Services.Services
{
    public class ChatService
    {
        private readonly ChatApiClient _apiClient;

        public ChatService()
        {
        }

        public ChatService(ChatApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<string> FindCompanionAsync(string gender, string city, string topic, string chat, int age) //(string gender, string city, string topic, int age)
        {
            var request = new CompanionRequest
            {
                Gender = gender,
                City = city,
                Topic = topic,
                Age = age,
                Chat = chat  
            };

            var response = await _apiClient.FindCompanionAsync(request);

            if (response.Success)
            {
                var companion = response.Data;
                return $"Найден компаньон: {companion.CompanionName} ({companion.CompanionGender}) по теме '{companion.CompanionTopic}' в городе '{companion.CompanionCity}'.";
            }

            return $"Ошибка: {response.Message}";
        }

        internal async Task<string> FindCompanionAsync(string genderSelection, string citySelection, string topicSelection, string chatSelections)
        {
            //throw new NotImplementedException(); красава бля просто эксепшен кинул а хули нет то
            return $"Найден собеседник ({genderSelection}) по теме '{topicSelection}' в городе '{citySelection}'! | '({chatSelections})'";
        }
    }
}