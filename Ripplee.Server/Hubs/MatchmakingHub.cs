using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Ripplee.Server.Models;
using Ripplee.Server.Services;
using System.Diagnostics;

namespace Ripplee.Server.Hubs
{
    [Authorize]
    public class MatchmakingHub : Hub
    {
        private readonly IMatchmakingService _matchmakingService;

        public MatchmakingHub(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        public async Task FindCompanion(string userCity, string searchGender, string searchCity, string searchTopic)
        {
            var username = Context.User.Identity?.Name ?? "Unknown";
            // Здесь в идеале надо получить пол пользователя из базы, а не доверять клиенту.
            // Но для простоты пока оставим так.
            var userGender = Context.User.FindFirst("gender")?.Value ?? "Не указан";

            var waitingUser = new WaitingUser
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                UserGender = userGender,
                UserCity = userCity,
                SearchGender = string.IsNullOrEmpty(searchGender) ? "Любой" : searchGender,
                SearchCity = searchCity,
                SearchTopic = searchTopic,
            };

            await _matchmakingService.AddUserToQueueAndTryMatchAsync(waitingUser);
        }
        public async Task FindAnyone()
        {
            await _matchmakingService.FindAnyMatchForUserAsync(Context.ConnectionId);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name ?? "Unknown";
            Debug.WriteLine($"--> User connected to MatchmakingHub: {username} ({Context.ConnectionId})");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _matchmakingService.RemoveUserFromQueueAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}