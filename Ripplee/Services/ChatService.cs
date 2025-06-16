using Ripplee.Services.Interfaces; 

namespace Ripplee.Services.Services
{
    // Реализуем интерфейс
    public class ChatService : IChatService
    {
        public async Task<string> FindCompanionAsync(string gender, string city, string topic, string chat)
        {
            await Task.Delay(20);
            return $"Идет поиск: ({gender}) по теме '{topic}' в городе '{city}' в чате '{chat}'!";
        }
    }
}