using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Booking
    {
        public int Id { get; set; } // Primary Key
        public int SpecialistId { get; set; } // Foreign Key to Specialist
        public int ClientId { get; set; } // Foreign Key to Client
        public int ServiceId { get; set; } // Foreign Key to Service
        public DateTime Day { get; set; } // Date of the booking
        public string TimeInterval { get; set; } // Time interval for booking
    }
}
