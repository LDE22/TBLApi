using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; } // Идентификатор специалиста
        public int ClientId { get; set; } // Идентификатор клиента
        public string Content { get; set; }
        public int Rating { get; set; } // Оценка (например, от 1 до 5)
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(SpecialistId))]
        public User Specialist { get; set; }

        [ForeignKey(nameof(ClientId))]
        public User Client { get; set; }
    }
}
