using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookTime(int id, [FromBody] BookingRequest request)
        {
            try
            {
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.SpecialistId == id && s.Day.Date == request.Day.Date);

                if (schedule == null)
                {
                    return NotFound(new { message = "Расписание не найдено." });
                }

                var newInterval = (Start: request.StartTime, End: request.EndTime);

                // Проверка пересечений
                foreach (var booked in schedule.BookedIntervalsList)
                {
                    if (booked.Start < newInterval.End && booked.End > newInterval.Start)
                    {
                        return Conflict(new { message = "Время уже занято." });
                    }
                }

                // Добавляем новое забронированное время
                schedule.BookedIntervalsList.Add(newInterval);

                _context.Schedules.Update(schedule);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Время успешно забронировано." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка при бронировании времени: {ex.Message}" });
            }
        }

        [HttpGet("specialist/{specialistId}")]
        public async Task<IActionResult> GetBookingsBySpecialist(int specialistId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Where(b => b.SpecialistId == specialistId)
                    .Include(b => b.Service)
                    .Include(b => b.Client)
                    .Select(b => new
                    {
                        b.Id,
                        b.SpecialistId,
                        b.ClientId,
                        b.ServiceId,
                        b.Day,
                        b.TimeInterval
                    })
                    .ToListAsync();

                var result = bookings.Select(b =>
                {
                    var times = ParseTimeInterval(b.TimeInterval);
                    return new
                    {
                        b.Id,
                        b.SpecialistId,
                        b.ClientId,
                        b.ServiceId,
                        b.Day,
                        b.TimeInterval,
                        StartTime = times.Start,
                        EndTime = times.End
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка при получении бронирований: {ex.Message}" });
            }
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetClientMeetings(int clientId)
        {
            try
            {
                var meetings = await _context.Bookings
                    .Where(b => b.ClientId == clientId)
                    .Select(b => new
                    {
                        b.Service.Title,
                        b.Service.Description,
                        b.Day,
                        StartTime = ParseTimeInterval(b.TimeInterval).Start,
                        EndTime = ParseTimeInterval(b.TimeInterval).End
                    })
                    .ToListAsync();

                return Ok(meetings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка при получении встреч: {ex.Message}" });
            }
        }

        [HttpGet("specialist/{id}/schedule")]
        public async Task<IActionResult> GetSpecialistSchedule(int id)
        {
            try
            {
                var schedule = await _context.Schedules
                    .Where(s => s.SpecialistId == id)
                    .Select(s => new
                    {
                        s.Id,
                        WorkingHours = s.WorkingHoursList, // Используем виртуальное свойство
                        s.BreakDuration,
                        s.Day
                    })
                    .ToListAsync();

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка при получении расписания: {ex.Message}" });
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

            throw new FormatException("TimeInterval имеет неверный формат. Ожидался формат 'HH:mm - HH:mm'.");
        }

        public class BookingRequest
        {
            public DateTime Day { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }
    }
}
