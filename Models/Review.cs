using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; } // ID специалиста
        public int ClientId { get; set; }     // ID клиента
        public string Content { get; set; }  // Текст отзыва
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        [ForeignKey(nameof(ClientId))]
        public User Client { get; set; }

        [ForeignKey(nameof(SpecialistId))]
        public User Specialist { get; set; }
    }
}
