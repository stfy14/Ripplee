using Microsoft.AspNetCore.SignalR;
using Ripplee.Server.Hubs; // Убедись, что путь к хабу правильный
using Ripplee.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Ripplee.Server.Services
{
    public interface IMatchmakingService
    {
        Task AddUserToQueueAndTryMatchAsync(WaitingUser user);
        Task FindAnyMatchForUserAsync(string connectionId);
        Task NotifyCallEndedByPartnerAsync(string connectionIdOfUserWhoEndedCall);
        Task LeaveCallGroupAsync(string connectionId);
        Task RemoveUserFromQueueAsync(string connectionId); // Используется при дисконнекте из очереди
    }

    public class MatchmakingService : IMatchmakingService
    {
        private static readonly ConcurrentDictionary<string, WaitingUser> _waitingUsers = new();
        private static readonly ConcurrentDictionary<string, string> _userCallGroups = new();
        private readonly IHubContext<MatchmakingHub> _hubContext;
        private readonly ILogger<MatchmakingService> _logger; // Добавим логгер

        // Убедись, что эти константы согласованы с теми, что в MatchmakingHub
        private string ANY_CRITERIA = string.Empty;
        private string DEFAULT_GENDER_IF_NOT_SET = string.Empty;


        public MatchmakingService(IHubContext<MatchmakingHub> hubContext, ILogger<MatchmakingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task AddUserToQueueAndTryMatchAsync(WaitingUser newUser)
        {
            _logger.LogInformation("MatchmakingService: User {Username} (ConnId: {ConnectionId}) added to queue. Criteria: UserGender={UserGender}, UserCity={UserCity}, SearchGender={SearchGender}, SearchCity={SearchCity}, SearchTopic={SearchTopic}",
                newUser.Username, newUser.ConnectionId, newUser.UserGender, newUser.UserCity, newUser.SearchGender, newUser.SearchCity, newUser.SearchTopic);

            var opponent = FindMatch(newUser);

            if (opponent != null)
            {
                _logger.LogInformation("MatchmakingService: Match found for {Username1} and {Username2}", newUser.Username, opponent.Username);
                await PairUsersAsync(newUser, opponent);
            }
            else
            {
                _waitingUsers[newUser.ConnectionId] = newUser;
                await _hubContext.Clients.Client(newUser.ConnectionId).SendAsync("SearchStatus", "Ищем собеседника по вашим критериям...");
                _logger.LogInformation("MatchmakingService: User {Username} is waiting. Total in queue: {QueueCount}", newUser.Username, _waitingUsers.Count);
            }
        }

        private WaitingUser? FindMatch(WaitingUser currentUser)
        {
            _logger.LogDebug("MatchmakingService: FindMatch called for User {Username} (ConnId: {ConnectionId})", currentUser.Username, currentUser.ConnectionId);
            foreach (var otherUser in _waitingUsers.Values.ToList()) // ToList для безопасности при возможном удалении
            {
                if (currentUser.ConnectionId == otherUser.ConnectionId) continue;

                _logger.LogDebug("MatchmakingService: Checking match between {CurrentUser} and {OtherUser}", currentUser.Username, otherUser.Username);
                bool currentUserLikesOtherUser = IsMatch(currentUser, otherUser);
                bool otherUserLikesCurrentUser = IsMatch(otherUser, currentUser);
                _logger.LogDebug("MatchmakingService: Match results - {CurrentUser} likes {OtherUser}: {Result1}; {OtherUser} likes {CurrentUser}: {Result2}",
                    currentUser.Username, otherUser.Username, currentUserLikesOtherUser, otherUser.Username, currentUser.Username, otherUserLikesCurrentUser);


                if (currentUserLikesOtherUser && otherUserLikesCurrentUser)
                {
                    return otherUser;
                }
            }
            return null;
        }

        private bool IsMatch(WaitingUser searchingUser, WaitingUser potentialPartner)
        {
            bool genderMatch = searchingUser.SearchGender == ANY_CRITERIA ||
                               searchingUser.SearchGender.Equals(potentialPartner.UserGender, StringComparison.OrdinalIgnoreCase);

            bool cityMatch = searchingUser.SearchCity == ANY_CRITERIA ||
                             searchingUser.SearchCity.Equals(potentialPartner.UserCity, StringComparison.OrdinalIgnoreCase);

            bool topicMatch = searchingUser.SearchTopic == ANY_CRITERIA ||
                              searchingUser.SearchTopic.Equals(potentialPartner.SearchTopic, StringComparison.OrdinalIgnoreCase);

            _logger.LogTrace("IsMatch Check: SearchingUser: {SearchingUser}, PotentialPartner: {PotentialPartner} | GenderMatch: {GenderMatch} (Criteria: {SGender} vs User: {PGender}), CityMatch: {CityMatch} (Criteria: {SCity} vs User: {PCity}), TopicMatch: {TopicMatch} (Criteria: {STopic} vs Search: {PTopic})",
                searchingUser.Username, potentialPartner.Username,
                genderMatch, searchingUser.SearchGender, potentialPartner.UserGender,
                cityMatch, searchingUser.SearchCity, potentialPartner.UserCity,
                topicMatch, searchingUser.SearchTopic, potentialPartner.SearchTopic);

            return genderMatch && cityMatch && topicMatch;
        }


        private async Task PairUsersAsync(WaitingUser user1, WaitingUser user2)
        {
            _waitingUsers.TryRemove(user1.ConnectionId, out _);
            _waitingUsers.TryRemove(user2.ConnectionId, out _);

            string callGroupId = Guid.NewGuid().ToString("N");

            await _hubContext.Groups.AddToGroupAsync(user1.ConnectionId, callGroupId);
            await _hubContext.Groups.AddToGroupAsync(user2.ConnectionId, callGroupId);

            _userCallGroups[user1.ConnectionId] = callGroupId;
            _userCallGroups[user2.ConnectionId] = callGroupId;

            _logger.LogInformation("PAIR FOUND & GROUPED: {User1} (Avatar: {Avatar1}) <-> {User2} (Avatar: {Avatar2}). Group: {CallGroupId}. Remaining in queue: {QueueCount}",
                user1.Username, user1.UserAvatarUrl, user2.Username, user2.UserAvatarUrl, callGroupId, _waitingUsers.Count);

            // Передаем URL аватара собеседника каждому клиенту
            await _hubContext.Clients.Client(user1.ConnectionId).SendAsync("CompanionFound",
                user2.Username,
                user2.UserCity,
                user2.SearchTopic, // Это тема, которую искал user2 (может совпадать с темой user1)
                callGroupId,
                user2.UserAvatarUrl); // Аватар user2

            await _hubContext.Clients.Client(user2.ConnectionId).SendAsync("CompanionFound",
                user1.Username,
                user1.UserCity,
                user1.SearchTopic, // Это тема, которую искал user1
                callGroupId,
                user1.UserAvatarUrl); // Аватар user1
        }

        public async Task FindAnyMatchForUserAsync(string connectionId)
        {
            if (!_waitingUsers.TryGetValue(connectionId, out var currentUser))
            {
                _logger.LogWarning("MatchmakingService: FindAnyMatchForUserAsync - User {ConnectionId} not found in waiting queue.", connectionId);
                return;
            }

            _logger.LogInformation("MatchmakingService: User {Username} (ConnId: {ConnectionId}) activated FindAnyone.", currentUser.Username, connectionId);
            currentUser.SearchGender = ANY_CRITERIA;
            currentUser.SearchCity = ANY_CRITERIA;
            currentUser.SearchTopic = ANY_CRITERIA;

            var opponent = FindMatch(currentUser);
            if (opponent != null)
            {
                _logger.LogInformation("MatchmakingService: FindAnyone - Match found for {Username1} and {Username2}", currentUser.Username, opponent.Username);
                await PairUsersAsync(currentUser, opponent);
            }
            else
            {
                await _hubContext.Clients.Client(currentUser.ConnectionId).SendAsync("SearchStatus", "Продолжаем поиск (любой собеседник)...");
                _logger.LogInformation("MatchmakingService: User {Username} still waiting after FindAnyone. Total in queue: {QueueCount}", currentUser.Username, _waitingUsers.Count);
            }
        }

        public async Task NotifyCallEndedByPartnerAsync(string connectionIdOfUserWhoEndedCall)
        {
            if (_userCallGroups.TryGetValue(connectionIdOfUserWhoEndedCall, out string? callGroupId))
            {
                _logger.LogInformation("MatchmakingService: Notifying call ended by partner for ConnId {ConnectionId} in group {CallGroupId}",
                    connectionIdOfUserWhoEndedCall, callGroupId);

                // Уведомляем другого участника в группе
                await _hubContext.Clients.GroupExcept(callGroupId, connectionIdOfUserWhoEndedCall).SendAsync("CallEndedByPartner");
            }
            else
            {
                _logger.LogWarning("MatchmakingService: NotifyCallEndedByPartnerAsync - User {ConnectionId} not found in any call group.", connectionIdOfUserWhoEndedCall);
            }
        }

        public async Task LeaveCallGroupAsync(string connectionId)
        {
            if (_userCallGroups.TryRemove(connectionId, out string? callGroupId))
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, callGroupId);
                _logger.LogInformation("MatchmakingService: User {ConnectionId} removed from call group {CallGroupId}.", connectionId, callGroupId);
            }
        }

        public Task RemoveUserFromQueueAsync(string connectionId) // Используется при дисконнекте, если пользователь был в очереди
        {
            if (_waitingUsers.TryRemove(connectionId, out var user))
            {
                _logger.LogInformation("MatchmakingService: User {Username} (ConnId: {ConnectionId}) removed from waiting queue due to disconnect or cancellation.",
                    user.Username, connectionId);
            }
            return Task.CompletedTask;
        }
    }
}