using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScheduleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{specialistId}")]
        public async Task<IActionResult> GetSchedule(int specialistId)
        {
            try
            {
                var schedules = await _context.Schedules
                    .Where(s => s.SpecialistId == specialistId)
                    .ToListAsync();

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке расписания: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule([FromBody] Schedule schedule)
        {
            if (schedule == null)
            {
                return BadRequest("Данные не могут быть пустыми.");
            }

            // Проверяем корректность JSON-форматов
            try
            {
                var workingHours = JsonSerializer.Deserialize<List<string>>(schedule.WorkingHours);
                var bookedIntervals = JsonSerializer.Deserialize<List<string>>(schedule.BookedIntervals);
            }
            catch (JsonException ex)
            {
                return BadRequest($"Неверный формат JSON: {ex.Message}");
            }

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return Ok(schedule);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, Schedule updatedSchedule)
        {
            if (id != updatedSchedule.Id)
                return BadRequest("ID расписания не совпадают.");

            try
            {
                var existingSchedule = await _context.Schedules.FindAsync(id);
                if (existingSchedule == null)
                    return NotFound("Расписание не найдено.");

                existingSchedule.Day = updatedSchedule.Day;
                existingSchedule.WorkingHours = updatedSchedule.WorkingHours;
                existingSchedule.BookedIntervals = updatedSchedule.BookedIntervals;

                await _context.SaveChangesAsync();
                return Ok(existingSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении расписания: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var schedule = await _context.Schedules.FindAsync(id);
                if (schedule == null)
                {
                    return NotFound("Расписание не найдено.");
                }

                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении расписания: {ex.Message}");
            }
        }
    }
}
