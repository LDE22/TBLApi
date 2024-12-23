namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ID жалобщика
        public int TargetId { get; set; } // ID цели
        public string Comment { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModeratorId { get; set; }
        public string ActionTaken { get; set; }

    }
}
