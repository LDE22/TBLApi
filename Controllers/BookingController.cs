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
        public async Task<IActionResult> BookTime([FromBody] BookingRequest request)
        {
            try
            {
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.SpecialistId == request.SpecialistId && s.Day.Date == request.Day.Date);

                if (schedule == null)
                {
                    return NotFound(new { message = "Расписание не найдено." });
                }

                var newInterval = (Start: request.StartTime, End: request.EndTime);

                // Проверяем пересечения с забронированными интервалами
                if (schedule.BookedIntervalsList.Any(booked => booked.Start < newInterval.End && booked.End > newInterval.Start))
                {
                    return Conflict(new { message = "Время уже занято." });
                }

                // Добавляем новый интервал
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
        public class BookingRequest
        {
            public int ClientId { get; set; }
            public int SpecialistId { get; set; } // ID специалиста
            public DateTime Day { get; set; } // Дата бронирования
            public TimeSpan StartTime { get; set; } // Начало бронирования
            public TimeSpan EndTime { get; set; } // Конец бронирования
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
                    .ToListAsync();

                var result = bookings.Select(b => new
                {
                    b.Id,
                    b.SpecialistId,
                    b.ClientId,
                    b.ServiceId,
                    b.Day,
                    b.TimeInterval,
                    StartTime = ParseTimeInterval(b.TimeInterval).Start,
                    EndTime = ParseTimeInterval(b.TimeInterval).End
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
                    .Include(b => b.Service)
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
    }
}
