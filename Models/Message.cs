using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Message
    {
        public int Id { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; } // Внешний ключ на User

        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; } // Внешний ключ на User

        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public User? Sender { get; set; } // Связь с отправителем
        public User? Receiver { get; set; } // Связь с получателем
    }
}
