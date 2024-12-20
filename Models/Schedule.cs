using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TBLApi.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json;

    public class Schedule
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; } // Foreign Key
        public DateTime Day { get; set; }

        [NotMapped]
        public List<string> WorkingHoursList
        {
            get => string.IsNullOrEmpty(WorkingHours) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(WorkingHours);
            set => WorkingHours = JsonSerializer.Serialize(value);
        }
        public string WorkingHours { get; set; } // JSON string

        [NotMapped]
        public List<string> BookedIntervalsList
        {
            get => string.IsNullOrEmpty(BookedIntervals) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(BookedIntervals);
            set => BookedIntervals = JsonSerializer.Serialize(value);
        }
        public string BookedIntervals { get; set; } // JSON string
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
