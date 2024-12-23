namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Кто подал жалобу
        public int TargetId { get; set; } // На кого подали жалобу
        public string Comment { get; set; } // Комментарий к жалобе
        public string Status { get; set; } = "Active"; // Статус жалобы
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Время создания жалобы
        public int ModeratorId { get; set; } // Модератор, обработавший жалобу (nullable)
        public string ActionTaken { get; set; } // Действие, принятое модератором
    }
}