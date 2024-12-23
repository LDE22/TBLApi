namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ComplainantId { get; set; } // ID ������������, ��� �����������
        public int TargetId { get; set; } // ID ������������, �� ���� ������
        public int ModeratorId { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; } = "��������"; // �������: "��������", "�������", "���������"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ActionTaken { get; set; } // �������� ����������: "�������������", "�����������", "���������"
    }

}
