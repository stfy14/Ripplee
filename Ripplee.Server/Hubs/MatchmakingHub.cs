using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Ripplee.Server.Models;
using Ripplee.Server.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace Ripplee.Server.Hubs
{
    [Authorize]
    public class MatchmakingHub : Hub
    {
        private readonly IMatchmakingService _matchmakingService;
        private const string ANY_CRITERIA_HUB = "Любой"; // Можно вынести в константу сервиса или общую

        public MatchmakingHub(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        public async Task FindCompanion(string userCity, string searchGender, string searchCity, string searchTopic)
        {
            var username = Context.User.Identity?.Name ?? "Unknown";
            string userGenderClaim = Context.User.FindFirst(ClaimTypes.Gender)?.Value;
            // userCity уже передается и должен быть актуальным городом пользователя

            var waitingUser = new WaitingUser
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                UserGender = userGenderClaim,
                UserCity = userCity, // Это город самого пользователя
                SearchGender = string.IsNullOrEmpty(searchGender) ? ANY_CRITERIA_HUB : searchGender,
                SearchCity = string.IsNullOrEmpty(searchCity) ? ANY_CRITERIA_HUB : searchCity, // Добавим обработку пустого значения
                SearchTopic = string.IsNullOrEmpty(searchTopic) ? ANY_CRITERIA_HUB : searchTopic, // Добавим обработку пустого значения
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