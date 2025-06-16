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

        public async Task FindCompanion(string userCity, string searchGender, string searchCity, string searchTopic)
        {
            var username = Context.User.Identity?.Name ?? "UnknownUser";
            string actualUserGender = Context.User.FindFirst(ClaimTypes.Gender)?.Value;
            string? userAvatarUrlFromClaim = Context.User.FindFirst("avatar_url")?.Value; 

            _logger.LogInformation("MatchmakingHub: User {Username} (ConnId: {ConnectionId}) called FindCompanion. Token Gender: '{TokenGender}', AvatarUrl: '{TokenAvatarUrl}'. Passed UserCity: {UserCity}",
                username, Context.ConnectionId, actualUserGender, userAvatarUrlFromClaim, userCity);

            if (string.IsNullOrEmpty(actualUserGender) || actualUserGender.Equals(DEFAULT_GENDER_IF_NOT_SET, StringComparison.OrdinalIgnoreCase))
            {
                actualUserGender = DEFAULT_GENDER_IF_NOT_SET;
            }
            _logger.LogInformation("MatchmakingHub: Effective UserGender for {Username}: '{EffectiveGender}'", username, actualUserGender);

            var waitingUser = new WaitingUser
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                UserGender = actualUserGender,
                UserCity = string.IsNullOrEmpty(userCity) ? DEFAULT_GENDER_IF_NOT_SET : userCity, 
                UserAvatarUrl = string.IsNullOrEmpty(userAvatarUrlFromClaim) ? null : userAvatarUrlFromClaim, 
                SearchGender = string.IsNullOrEmpty(searchGender) ? ANY_CRITERIA_HUB : searchGender,
                SearchCity = string.IsNullOrEmpty(searchCity) ? ANY_CRITERIA_HUB : searchCity,
                SearchTopic = string.IsNullOrEmpty(searchTopic) ? ANY_CRITERIA_HUB : searchTopic,
            };
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
    }
}