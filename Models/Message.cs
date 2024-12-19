using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Message
    {
        public int Id { get; set; }

        [ForeignKey("Sender")]
        public int SenderId { get; set; } // ������� ���� �� User

        [ForeignKey("Receiver")]
        public int ReceiverId { get; set; } // ������� ���� �� User

        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public User? Sender { get; set; } // ����� � ������������
        public User? Receiver { get; set; } // ����� � �����������
    }
}
