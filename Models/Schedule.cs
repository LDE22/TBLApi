using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TBLApi.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public string Day { get; set; }
        public string WorkingHours { get; set; }
        public List<string> BookedIntervals { get; set; } = new();
    }

    public class Booking
    {
        public int SpecialistId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public string Day { get; set; }
        public string TimeInterval { get; set; } // Пример: "10:00-10:30"
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public string Day { get; set; }
        public string TimeInterval { get; set; }
    }
}
