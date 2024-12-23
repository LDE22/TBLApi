using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public string Day { get; set; }
        public string TimeInterval { get; set; }
    }
}
