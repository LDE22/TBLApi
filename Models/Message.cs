namespace TBLApi.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; } // ������� ���� �� User
        public int ReceiverId { get; set; } // ������� ���� �� User
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }

        public User? Sender { get; set; } // ����� � ������������
        public User? Receiver { get; set; } // ����� � �����������
    }
}
