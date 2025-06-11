// Ripplee.Server/Hubs/MatchmakingHub.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace Ripplee.Server.Hubs
{
    // ✅ ЗАЩИЩАЕМ HUB: Только авторизованные пользователи смогут сюда подключиться.
    [Authorize]
    public class MatchmakingHub : Hub
    {
        // Этот метод будет вызываться из нашего MAUI-клиента
        public async Task FindCompanion(string gender, string city, string topic)
        {
            // Context.User.Identity.Name содержит имя пользователя из JWT токена.
            var username = Context.User?.Identity?.Name ?? "Unknown";

            // Пока что просто выводим в консоль, что запрос получен.
            // В будущем здесь будет сложная логика поиска.
            Debug.WriteLine($"User '{username}' is looking for a companion with criteria: Gender={gender}, City={city}, Topic={topic}");

            // TODO: Реализовать логику добавления в очередь и поиска пары.

            // Пример того, как сервер может вызвать метод на клиенте, который отправил запрос:
            await Clients.Caller.SendAsync("SearchStatus", "Поиск начат. Ожидайте...");
        }

        // Этот метод вызывается автоматически, когда клиент успешно подключается.
        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name ?? "Unknown";
            Debug.WriteLine($"--> User connected to MatchmakingHub: {username}");
            await base.OnConnectedAsync();
        }

        // Этот метод вызывается, когда клиент отключается.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name ?? "Unknown";
            Debug.WriteLine($"<-- User disconnected from MatchmakingHub: {username}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}