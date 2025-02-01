using System.Threading.Tasks;

namespace Ripplee.Services
{
    public class ChatService
    {
        public async Task<string> FindCompanionAsync(string gender, string city, string topic)
        {
            await Task.Delay(500); // Имитация запроса к серверу
            return $"Найден собеседник ({gender}) по теме '{topic}' в городе '{city}'!";
        }
    }
}
