using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; } // Идентификатор чата
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
