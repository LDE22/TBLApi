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
        public async Task<IActionResult> AddBooking([FromBody] Booking booking)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(booking.TimeInterval))
                {
                    var times = ParseTimeInterval(booking.TimeInterval);
                    booking.StartTime = times.StartTime;
                    booking.EndTime = times.EndTime;
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Запись успешно создана." });
            }
            catch (FormatException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("specialist/{specialistId}")]
        public async Task<IActionResult> GetBookingsBySpecialist(int specialistId)
        {
            // Извлекаем данные из базы только с доступными полями
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
                    b.TimeInterval // Извлекаем TimeInterval из базы
                })
                .ToListAsync();

            // Обрабатываем TimeInterval для извлечения StartTime и EndTime
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
                    StartTime = times.StartTime, // Извлекаем StartTime
                    EndTime = times.EndTime      // Извлекаем EndTime
                };
            });

            return Ok(result);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetClientMeetings(int clientId)
        {
            var meetings = await _context.Bookings
                .Where(b => b.ClientId == clientId)
                .Select(b => new
                {
                    Title = b.Service.Title,
                    Description = b.Service.Description,
                    Day = b.Day,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime
                })
                .ToListAsync();

            return Ok(meetings);
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
                        s.WorkingHours,
                        s.BreakDuration,
                        s.Day
                    })
                    .ToListAsync();

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении расписания: {ex.Message}");
            }
        }
        private static (TimeSpan StartTime, TimeSpan EndTime) ParseTimeInterval(string timeInterval)
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