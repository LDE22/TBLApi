namespace TBLApi.Models
{
    public class ServiceModel
    {
        public int Id { get; set; }
        public string Title { get; set; } // Исправлено
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int SpecialistId { get; set; }
        public string City { get; set; } // Добавлено
        public string SpecialistName { get; set; } // Добавлено

        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
