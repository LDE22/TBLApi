namespace TBLApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; } // ID специалиста, которому оставлен отзыв
        public int ClientId { get; set; } // ID клиента, оставившего отзыв
        public string Content { get; set; } // Текст отзыва
        public int Rating { get; set; } // Оценка (например, от 1 до 5)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Дата и время отзыва

        // Связь с таблицами Users
        public User Specialist { get; set; } // Навигационное свойство для специалиста
        public User Client { get; set; } // Навигационное свойство для клиента
    }


}
