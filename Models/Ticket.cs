namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TargetId { get; set; }

        public string Comment { get; set; } // ������ ������� [Required]

        public string ActionTaken { get; set; } // ������ ������� [Required]

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModeratorId { get; set; }
    }
}
