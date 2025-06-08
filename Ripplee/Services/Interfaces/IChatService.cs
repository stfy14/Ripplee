namespace Ripplee.Services.Interfaces
{
    public interface IChatService
    {
        Task<string> FindCompanionAsync(string gender, string city, string topic, string chat);
    }
}