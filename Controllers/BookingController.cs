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
                Console.WriteLine($"Запрос на бронирование: {JsonSerializer.Serialize(request)}");

                // Поиск расписания
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.SpecialistId == request.SpecialistId && s.Day == request.Day);

                if (schedule == null)
                {
                    return NotFound(new { message = "Расписание не найдено." });
                }

                // Проверка пересечений
                var newInterval = (Start: request.StartTime, End: request.EndTime);
                if (schedule.BookedIntervalsList.Any(booked => booked.Start < newInterval.End && booked.End > newInterval.Start))
                {
                    return Conflict(new { message = "Время уже занято." });
                }

                // Создание записи
                var booking = new Booking
                {
                    SpecialistId = request.SpecialistId,
                    ClientId = request.ClientId,
                    ServiceId = request.ServiceId,
                    Day = request.Day,
                    TimeInterval = $"{request.StartTime:hh\\:mm}-{request.EndTime:hh\\:mm}"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Время успешно забронировано." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return StatusCode(500, new { message = $"Ошибка при бронировании времени: {ex.Message}" });
            }
        }

        public class BookingRequest
        {
            public int ClientId { get; set; }
            public int SpecialistId { get; set; }
            public int ServiceId { get; set; }
            public DateOnly Day { get; set; } // Чистая дата
            public TimeSpan StartTime { get; set; } // Начало бронирования
            public TimeSpan EndTime { get; set; }
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
                        b.Id,
                        b.SpecialistId,
                        b.ClientId,
                        b.ServiceId,
                        b.Day,
                        b.TimeInterval,
                    })
                    .ToListAsync();

                return Ok(meetings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка при получении встреч: {ex.Message}" });
            }
        }
    }
}
