using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Chat
    {
        public int Id { get; set; } // Идентификатор чата
        public int SenderId { get; set; } // Отправитель чата
        public int ReceiverId { get; set; } // Получатель чата
        public string LastMessage { get; set; } // Последнее сообщение
        public DateTime Timestamp { get; set; } // Время последнего сообщения

        [ForeignKey(nameof(SenderId))]
        public User Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public User Receiver { get; set; }
    }
}
