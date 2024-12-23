namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ID ���������
        public int TargetId { get; set; } // ID ����
        public string Comment { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModeratorId { get; set; }
        public string ActionTaken { get; set; }

        // ����������� ���� ��� �����������
        public string ComplainantName { get; set; }
        public string ComplainantPhoto { get; set; }
        public string TargetName { get; set; }
        public string TargetPhoto { get; set; }
    }
}
