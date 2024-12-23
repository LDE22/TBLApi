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
            if (booking.SpecialistId <= 0 || booking.ClientId <= 0 || booking.ServiceId <= 0)
            {
                return BadRequest(new { message = "Все обязательные поля должны быть заполнены." });
            }

            if (string.IsNullOrWhiteSpace(booking.TimeInterval))
            {
                return BadRequest(new { message = "Временной интервал не указан." });
            }

            try
            {
                booking.Day = booking.Day.ToUniversalTime();
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при бронировании: {ex.Message}");
            }
        }


        [HttpGet("specialist/{id}")]
        public async Task<IActionResult> GetSpecialistOrders(int id)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Where(b => b.SpecialistId == id)
                    .Select(b => new
                    {
                        b.Id,
                        b.SpecialistId,
                        b.ClientId,
                        b.ServiceId,
                        b.Day,
                        b.TimeInterval,
                        Title = b.Service.Title,
                        Description = b.Service.Description,
                        ClientName = b.Client.Name, // Связь с клиентом
                        ClientPhoto = b.Client.PhotoBase64 // Фото клиента
                    })
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке заказов: {ex.Message}");
            }
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
