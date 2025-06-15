using System;
using System.Threading.Tasks;

namespace Ripplee.Services.Interfaces
{
    public interface ISignalRService
    {
        Task ConnectAsync(Func<string, string, string, Task> onCompanionFound, Func<string, Task> onSearchStatus);
        Task DisconnectAsync();
        Task FindCompanionAsync(string userCity, string searchGender, string searchCity, string searchTopic);
        Task FindAnyoneAsync();
        bool IsConnected { get; }
    }
}