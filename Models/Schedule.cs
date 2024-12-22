using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TBLApi.Models
{


    public class Schedule
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public string Day { get; set; } // Например, "Monday"
        public string WorkingHours { get; set; } // JSON-строка: ["09:00-12:00", "13:00-18:00"]
        public string BookedIntervals { get; set; } // JSON-строка: ["10:00-11:00", "15:00-16:00"]

        // Виртуальные свойства для удобства работы
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
}
