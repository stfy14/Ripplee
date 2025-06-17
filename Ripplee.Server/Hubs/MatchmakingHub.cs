using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Ripplee.Server.Models;
using Ripplee.Server.Services;
using System; 
using System.Security.Claims; 
using System.Threading.Tasks; 
using Microsoft.Extensions.Logging; 

namespace Ripplee.Server.Hubs
{
    [Authorize]
    public class MatchmakingHub : Hub
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly ILogger<MatchmakingHub> _logger;

        private string ANY_CRITERIA_HUB = string.Empty;
        private string DEFAULT_GENDER_IF_NOT_SET = string.Empty;

        public MatchmakingHub(IMatchmakingService matchmakingService, ILogger<MatchmakingHub> logger)
        {
            _matchmakingService = matchmakingService;
            _logger = logger;
        }

        public async Task FindCompanion(string userCity, string userGender, string searchGender, string searchCity, string searchTopic, string chatType) 
        {
            var username = Context.User.Identity?.Name ?? "UnknownUser";
            string? userAvatarUrlFromClaim = Context.User.FindFirst("avatar_url")?.Value;

            _logger.LogInformation("MatchmakingHub: User {Username} (ConnId: {ConnectionId}) called FindCompanion. " +
                                 "Client-Side UserGender: '{ClientUserGender}', Client-Side UserCity: {UserCity}, ChatType: {ChatType}. " +
                                 "SearchCriteria: Gender='{SearchGender}', City='{SearchCity}', Topic='{SearchTopic}'",
                username, Context.ConnectionId, userGender, userCity, chatType, searchGender, searchCity, searchTopic);

            // Валидация полученного userGender
            string validatedUserGender = (string.IsNullOrEmpty(userGender) || userGender.Equals(DEFAULT_GENDER_IF_NOT_SET, StringComparison.OrdinalIgnoreCase))
                                         ? DEFAULT_GENDER_IF_NOT_SET
                                         : userGender;

            var waitingUser = new WaitingUser
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                UserGender = validatedUserGender, // Используем переданный и провалидированный пол
                UserCity = string.IsNullOrEmpty(userCity) ? DEFAULT_GENDER_IF_NOT_SET : userCity,
                UserAvatarUrl = string.IsNullOrEmpty(userAvatarUrlFromClaim) ? null : userAvatarUrlFromClaim,
                SearchGender = string.IsNullOrEmpty(searchGender) ? ANY_CRITERIA_HUB : searchGender,
                SearchCity = string.IsNullOrEmpty(searchCity) ? ANY_CRITERIA_HUB : searchCity,
                SearchTopic = string.IsNullOrEmpty(searchTopic) ? ANY_CRITERIA_HUB : searchTopic,
                // ChatType можно добавить в WaitingUser, если логика подбора в MatchmakingService будет от него зависеть
                // public string ChatType { get; set; }
            };
            // Если ChatType важен для MatchmakingService, передай его:
            // await _matchmakingService.AddUserToQueueAndTryMatchAsync(waitingUser, chatType);
            // Иначе, если MatchmakingService не учитывает тип чата при подборе, то он просто для информации.
            await _matchmakingService.AddUserToQueueAndTryMatchAsync(waitingUser);
        }

        public async Task FindAnyone()
        {
            _logger.LogInformation("MatchmakingHub: User {ConnectionId} called FindAnyone.", Context.ConnectionId);
            await _matchmakingService.FindAnyMatchForUserAsync(Context.ConnectionId);
        }

        public async Task EndCallNotification()
        {
            _logger.LogInformation("MatchmakingHub: User {ConnectionId} initiated EndCallNotification.", Context.ConnectionId);
            await _matchmakingService.NotifyCallEndedByPartnerAsync(Context.ConnectionId);
            await _matchmakingService.LeaveCallGroupAsync(Context.ConnectionId); 
        }

        public async Task ToggleMuteStatus(bool isMuted, string callGroupId)
        {
            if (string.IsNullOrEmpty(callGroupId))
            {
                _logger.LogWarning("MatchmakingHub: ToggleMuteStatus received with empty callGroupId from {ConnectionId}.", Context.ConnectionId);
                return;
            }

            _logger.LogInformation("MatchmakingHub: User {ConnectionId} in group {CallGroupId} toggled mute status to {IsMuted}", 
                Context.ConnectionId, callGroupId, isMuted);

            await Clients.GroupExcept(callGroupId, Context.ConnectionId).SendAsync("PartnerMuteStatusChanged", isMuted);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name ?? "UnknownUser";
            _logger.LogInformation("--> User connected to MatchmakingHub: {Username} ({ConnectionId})", username, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name ?? "UnknownUser"; 
            _logger.LogInformation("<-- User {Username} ({ConnectionId}) disconnected from MatchmakingHub. Exception: {ExceptionMessage}", 
                username, Context.ConnectionId, exception?.Message ?? "N/A");
            
            await _matchmakingService.LeaveCallGroupAsync(Context.ConnectionId); 
            await _matchmakingService.RemoveUserFromQueueAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTextMessage(string callGroupId, string messageText, string senderAvatarUrl) // Добавили senderAvatarUrl
        {
            if (string.IsNullOrEmpty(callGroupId) || string.IsNullOrEmpty(messageText))
            {
                _logger.LogWarning("MatchmakingHub: SendTextMessage received with empty callGroupId or messageText from {ConnectionId}.", Context.ConnectionId);
                return;
            }

            var senderUsername = Context.User.Identity?.Name ?? "UnknownUser";
            // string? senderAvatarUrl = Context.User.FindFirst("avatar_url")?.Value; // Уже получаем от клиента

            _logger.LogInformation("MatchmakingHub: User {Username} ({ConnectionId}) in group {CallGroupId} sent message: '{MessageText}' (Avatar: {Avatar})",
                senderUsername, Context.ConnectionId, callGroupId, messageText, senderAvatarUrl);

            // Отправляем сообщение другому участнику(ам) в группе
            // Вместе с сообщением передаем имя отправителя и его аватар
            await Clients.GroupExcept(callGroupId, Context.ConnectionId).SendAsync("ReceiveTextMessage", senderUsername, messageText, senderAvatarUrl);
        }
    }
}