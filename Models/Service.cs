using System.ComponentModel.DataAnnotations;

namespace TBLApi.Models
{
    public class ServiceModel
    {
        public int Id { get; set; }
        public required string Title { get; set; } // Исправлено
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int SpecialistId { get; set; }
        public string City { get; set; }
        public string SpecialistName { get; set; }
        public int Duration { get; set; }


        [Required]
        public List<Review> Reviews { get; set; } = new List<Review>();

        [Required]
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
