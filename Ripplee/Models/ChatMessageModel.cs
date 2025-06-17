using CommunityToolkit.Mvvm.ComponentModel; 

namespace Ripplee.Models
{
    public enum MessageSenderType
    {
        CurrentUser, 
        Companion    
    }

    public partial class ChatMessageModel 
    {
        public string? MessageId { get; set; } // Уникальный ID сообщения (Guid.NewGuid().ToString())
        public string? Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SenderUsername { get; set; } // Имя отправителя (для информации, или если в группе >2)
        public MessageSenderType SenderType { get; set; } // Тип отправителя для выравнивания в UI
        public string? AvatarUrl { get; set; } // URL аватара отправителя

        // Конструктор для удобства
        public ChatMessageModel(string text, MessageSenderType senderType, string? senderUsername = null, string? avatarUrl = null)
        {
            MessageId = Guid.NewGuid().ToString();
            Text = text;
            Timestamp = DateTime.Now;
            SenderType = senderType;
            SenderUsername = senderUsername;
            AvatarUrl = avatarUrl;
        }
    }
}