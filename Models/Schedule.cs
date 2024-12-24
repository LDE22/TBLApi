using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TBLApi.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public DateTime Day { get; set; } // Убедитесь, что клиент передаёт дату в ISO 8601 формате
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakDuration { get; set; }
        [NotMapped]
        public string BookedIntervals { get; set; } = "[]";// Занятые интервалы в формате JSON
        [NotMapped]
        public string WorkingHours { get; set; } = "[]";

        [NotMapped]
        public List<(TimeSpan Start, TimeSpan End)> WorkingHoursList
        {
            get
            {
                if (string.IsNullOrEmpty(WorkingHours))
                    return new List<(TimeSpan, TimeSpan)>();

                try
                {
                    // Десериализация строкового списка интервалов
                    var intervals = JsonSerializer.Deserialize<List<string>>(WorkingHours, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return intervals.Select(ParseTimeInterval).ToList();
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка десериализации WorkingHours: {ex.Message}");
                    return new List<(TimeSpan, TimeSpan)>();
                }
            }
            set
            {
                // Сериализация интервалов обратно в строку
                WorkingHours = JsonSerializer.Serialize(value.Select(i => $"{i.Start:hh\\:mm} - {i.End:hh\\:mm}"));
            }
        }

        [NotMapped]
        public List<(TimeSpan Start, TimeSpan End)> BookedIntervalsList
        {
            get => string.IsNullOrEmpty(BookedIntervals)
                ? new List<(TimeSpan Start, TimeSpan End)>()
                : JsonSerializer.Deserialize<List<(TimeSpan Start, TimeSpan End)>>(BookedIntervals);
            set => BookedIntervals = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<(TimeSpan Start, TimeSpan End)> AvailableIntervals
        {
            get
            {
                var available = new List<(TimeSpan Start, TimeSpan End)>();
                var currentTime = StartTime;

                while (currentTime + TimeSpan.FromMinutes(BreakDuration) < EndTime)
                {
                    var nextTime = currentTime + TimeSpan.FromMinutes(BreakDuration);

                    // Проверка на пересечение с BookedIntervals
                    if (!BookedIntervalsList.Any(b =>
                            b.Start < nextTime && b.End > currentTime))
                    {
                        available.Add((currentTime, nextTime));
                    }

                    currentTime = nextTime;
                }

                return available;
            }
        }
        private static (TimeSpan Start, TimeSpan End) ParseTimeInterval(string timeInterval)
        {
            var parts = timeInterval.Split('-');
            if (parts.Length == 2 &&
                TimeSpan.TryParse(parts[0].Trim(), out var start) &&
                TimeSpan.TryParse(parts[1].Trim(), out var end))
            {
                return (start, end);
            }

            throw new FormatException("Неверный формат интервала времени. Ожидается 'HH:mm - HH:mm'.");
        }
    }
}
