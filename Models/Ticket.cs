namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ComplainantId { get; set; }
        public int TargetId { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModeratorId { get; set; } // Сделать nullable
        public string ActionTaken { get; set; }
    }
}