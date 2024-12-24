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
        public DateTime Day { get; set; } // Убедитесь, что это DateTime, если клиент отправляет дату
        public string WorkingHours { get; set; }
        public string BookedIntervals { get; set; }
        public int BreakDuration { get; set; } // Добавить в модель Schedule
    }
}
