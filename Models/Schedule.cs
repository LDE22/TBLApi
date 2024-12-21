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
        public DateTime Day { get; set; } // Например, "2024-12-21T00:00:00"
        public string WorkingHours { get; set; } // Например, "[\"09:00-13:00\", \"14:00-18:00\"]"
        public string BookedIntervals { get; set; } // Например, "[\"09:00-10:00\", \"15:00-16:00\"]"

        [NotMapped]
        public List<string> WorkingHoursList
        {
            get
            {
                try
                {
                    // Проверяем, не пусто ли значение, и десериализуем строку JSON в List<string>
                    return string.IsNullOrEmpty(WorkingHours)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(WorkingHours);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка десериализации WorkingHours: {ex.Message}");
                    return new List<string>();
                }
            }
            set
            {
                // Сериализация списка обратно в JSON
                WorkingHours = JsonSerializer.Serialize(value);
            }
        }

        [NotMapped]
        public List<string> BookedIntervalsList
        {
            get
            {
                try
                {
                    return string.IsNullOrEmpty(BookedIntervals)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(BookedIntervals);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка десериализации BookedIntervals: {ex.Message}");
                    return new List<string>();
                }
            }
            set
            {
                BookedIntervals = JsonSerializer.Serialize(value);
            }
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
