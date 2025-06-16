using System;
using System.Threading.Tasks;

namespace Ripplee.Services.Interfaces
{
    public interface ISignalRService
    {
        Task ConnectAsync(
            Func<string, string, string, string, string?, Task> onCompanionFound, // name, city, topic, callGroupId, companionAvatarUrl?
            Func<string, Task> onSearchStatus,
            Func<Task> onCallEndedByPartner,
            Func<bool, Task> onPartnerMuteStatusChanged
        );
        Task DisconnectAsync();
        Task FindCompanionAsync(string userCity, string searchGender, string searchCity, string searchTopic);
        Task FindAnyoneAsync();
        Task NotifyEndCallAsync();
        Task LeaveCurrentCallGroupAsync();
        Task SendMuteStatusAsync(bool isMuted);
        bool IsConnected { get; }
        string? CurrentCallGroupId { get; }
    }
}