namespace TBLApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ComplainantId { get; set; } // ID пользователя, кто пожаловался
        public int TargetId { get; set; } // ID пользователя, на кого жалоба
        public int ModeratorId { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; } = "Ожидание"; // Статусы: "Ожидание", "Принято", "Отклонено"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ActionTaken { get; set; } // Действие модератора: "Заблокировать", "Ограничение", "Отклонено"
    }

}
