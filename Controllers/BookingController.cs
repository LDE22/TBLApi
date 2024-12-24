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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Проверка доступности времени
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.SpecialistId == booking.SpecialistId && s.Day.Date == booking.Day.Date);

            if (schedule == null) return BadRequest(new { message = "Расписание на выбранную дату не найдено." });

            var bookedIntervals = JsonSerializer.Deserialize<List<string>>(schedule.BookedIntervals) ?? new List<string>();

            var bookingInterval = $"{booking.StartTime}-{booking.EndTime}";

            if (bookedIntervals.Contains(bookingInterval))
            {
                return BadRequest(new { message = "Выбранное время уже занято." });
            }

            bookedIntervals.Add(bookingInterval);
            schedule.BookedIntervals = JsonSerializer.Serialize(bookedIntervals);

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Запись успешно создана." });
        }

        [HttpGet("specialist/{specialistId}")]
        public async Task<IActionResult> GetBookingsBySpecialist(int specialistId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.SpecialistId == specialistId)
                .Include(b => b.Service)
                .Include(b => b.Client)
                .ToListAsync();
            return Ok(bookings);
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
    }
}