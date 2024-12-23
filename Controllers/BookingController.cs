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
        public async Task<IActionResult> BookService(Booking booking)
        {
            try
            {
                // Приведение даты к UTC
                if (booking.Day.Kind == DateTimeKind.Unspecified)
                {
                    booking.Day = DateTime.SpecifyKind(booking.Day, DateTimeKind.Utc);
                }

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при бронировании: {ex.Message}, {ex.InnerException?.Message}");
            }
        }

        [HttpGet("specialist/{specialistId}")]
        public async Task<IActionResult> GetSpecialistOrders(int specialistId)
        {
            var orders = await _context.Bookings
                .Where(b => b.SpecialistId == specialistId)
                .Select(b => new
                {
                    Title = b.Service.Title,
                    Description = b.Service.Description,
                    Day = b.Day,
                    TimeInterval = b.TimeInterval
                })
                .ToListAsync();

            return Ok(orders);
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
                    TimeInterval = b.TimeInterval
                })
                .ToListAsync();

            return Ok(meetings);
        }
    }
}
