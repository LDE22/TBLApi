namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ��� ����� ������
        public int TargetId { get; set; } // �� ���� ������ ������
        public string Comment { get; set; } // ����������� � ������
        public string Status { get; set; } = "Active"; // ������ ������
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ����� �������� ������
        public int ModeratorId { get; set; } // ���������, ������������ ������ (nullable)
        public string ActionTaken { get; set; } // ��������, �������� �����������
    }
}