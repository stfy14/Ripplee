namespace Ripplee.Server.Models
{
    // Информация о пользователе, который находится в очереди на подбор
    public class WaitingUser
    {
        // Техническая информация
        public required string ConnectionId { get; set; } // ID подключения SignalR, чтобы мы могли с ним связаться
        public required string Username { get; set; }

        // Информация о самом пользователе (чтобы другие могли его найти)
        public required string UserGender { get; set; } // Его собственный пол
        public required string UserCity { get; set; }   // Его собственный город

        // Критерии поиска (кого он ищет)
        public required string SearchGender { get; set; }
        public required string SearchCity { get; set; }
        public required string SearchTopic { get; set; }
    }
}