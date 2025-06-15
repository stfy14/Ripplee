using Microsoft.AspNetCore.SignalR;
using Ripplee.Server.Hubs;
using Ripplee.Server.Models;
using System.Collections.Concurrent;

namespace Ripplee.Server.Services
{
    // ИНТЕРФЕЙС: Определен здесь, в одном месте.
    public interface IMatchmakingService
    {
        Task AddUserToQueueAndTryMatchAsync(WaitingUser user);
        Task FindAnyMatchForUserAsync(string connectionId);
        Task RemoveUserFromQueueAsync(string connectionId);
    }

    // КЛАСС: Реализует интерфейс, объявленный ВЫШЕ.
    public class MatchmakingService : IMatchmakingService
    {
        private static readonly ConcurrentDictionary<string, WaitingUser> _waitingUsers = new();
        private readonly IHubContext<MatchmakingHub> _hubContext;
        private const string ANY_CRITERIA = "Любой";

        public MatchmakingService(IHubContext<MatchmakingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task AddUserToQueueAndTryMatchAsync(WaitingUser newUser)
        {
            var opponent = FindMatch(newUser);

            if (opponent != null)
            {
                await PairUsersAsync(newUser, opponent);
            }
            else
            {
                _waitingUsers[newUser.ConnectionId] = newUser;
                await _hubContext.Clients.Client(newUser.ConnectionId).SendAsync("SearchStatus", "Ищем собеседника по вашим критериям...");
                Console.WriteLine($"User {newUser.Username} added to queue. Total: {_waitingUsers.Count}");
            }
        }

        public async Task FindAnyMatchForUserAsync(string connectionId)
        {
            if (!_waitingUsers.TryGetValue(connectionId, out var currentUser))
            {
                return;
            }

            currentUser.SearchGender = ANY_CRITERIA;
            currentUser.SearchCity = ANY_CRITERIA;
            currentUser.SearchTopic = ANY_CRITERIA;

            var opponent = FindMatch(currentUser);
            if (opponent != null)
            {
                await PairUsersAsync(currentUser, opponent);
            }
            else
            {
                await _hubContext.Clients.Client(currentUser.ConnectionId).SendAsync("SearchStatus", "Продолжаем поиск, но уже по любым критериям...");
                Console.WriteLine($"User {currentUser.Username} widened search. Still waiting. Total: {_waitingUsers.Count}");
            }
        }

        public Task RemoveUserFromQueueAsync(string connectionId)
        {
            if (_waitingUsers.TryRemove(connectionId, out var user))
            {
                Console.WriteLine($"User {user.Username} disconnected and removed from queue.");
            }
            return Task.CompletedTask;
        }

        private async Task PairUsersAsync(WaitingUser user1, WaitingUser user2)
        {
            _waitingUsers.TryRemove(user1.ConnectionId, out _);
            _waitingUsers.TryRemove(user2.ConnectionId, out _);

            Console.WriteLine($"PAIR FOUND: {user1.Username} <-> {user2.Username}");
            await NotifyPairFoundAsync(user1, user2);
        }

        private WaitingUser? FindMatch(WaitingUser currentUser)
        {
            foreach (var otherUser in _waitingUsers.Values)
            {
                if (currentUser.ConnectionId == otherUser.ConnectionId) continue;
                if (IsMatch(currentUser, otherUser) && IsMatch(otherUser, currentUser))
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

            // Если подбор по общей теме поиска:
            bool topicMatch = searchingUser.SearchTopic == ANY_CRITERIA ||
                              searchingUser.SearchTopic.Equals(potentialPartner.SearchTopic, StringComparison.OrdinalIgnoreCase);

            return genderMatch && cityMatch && topicMatch;
        }

        private async Task NotifyPairFoundAsync(WaitingUser user1, WaitingUser user2)
        {
            await _hubContext.Clients.Client(user1.ConnectionId).SendAsync("CompanionFound", user2.Username, user2.UserCity, user2.SearchTopic);
            await _hubContext.Clients.Client(user2.ConnectionId).SendAsync("CompanionFound", user1.Username, user1.UserCity, user1.SearchTopic);
        }
    }
}