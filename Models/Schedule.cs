using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TBLApi.Models
{


    public class Schedule
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; } // Foreign Key
        public DateTime Day { get; set; }

        // Список рабочих часов в виде JSON
        public string WorkingHours { get; set; } // Хранится как JSON
        public string BookedIntervals { get; set; } // Хранится как JSON

        // Помощники для работы с JSON
        [NotMapped]
        public List<string> WorkingHoursList
        {
            get => string.IsNullOrEmpty(WorkingHours) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(WorkingHours);
            set => WorkingHours = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> BookedIntervalsList
        {
            get => string.IsNullOrEmpty(BookedIntervals) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(BookedIntervals);
            set => BookedIntervals = JsonSerializer.Serialize(value);
        }
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
