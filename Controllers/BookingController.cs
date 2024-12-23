﻿using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> BookService([FromBody] Booking booking)
        {
            try
            {
                // Добавляем запись в базу данных
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Возвращаем созданную запись
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при бронировании: {ex.Message}");
            }
        }

        [HttpGet("specialist/{specialistId}")]
        public async Task<IActionResult> GetSpecialistOrders(int specialistId)
        {
            try
            {
                var orders = await _context.Bookings
                    .Where(b => b.SpecialistId == specialistId)
                    .ToListAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке заказов: {ex.Message}");
            }
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetClientMeetings(int clientId)
        {
            try
            {
                var meetings = await _context.Bookings
                    .Where(b => b.ClientId == clientId)
                    .ToListAsync();
                return Ok(meetings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке встреч: {ex.Message}");
            }
        }
    }
}