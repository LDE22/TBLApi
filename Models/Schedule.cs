using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TBLApi.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }

        // Поле Day должно быть DateTime
        public DateTime Day { get; set; }

        // JSON для рабочих часов
        public string WorkingHours { get; set; } // Пример: ["09:00-13:00", "14:00-18:00"]

        // JSON для занятых интервалов
        public string BookedIntervals { get; set; } // Пример: ["09:00-10:00", "15:00-16:00"]

        // Удобные свойства для работы с JSON
        [NotMapped]
        public List<string> WorkingHoursList
        {
            get => string.IsNullOrEmpty(WorkingHours)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(WorkingHours);
            set => WorkingHours = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> BookedIntervalsList
        {
            get => string.IsNullOrEmpty(BookedIntervals)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(BookedIntervals);
            set => BookedIntervals = JsonSerializer.Serialize(value);
        }
    }
}
