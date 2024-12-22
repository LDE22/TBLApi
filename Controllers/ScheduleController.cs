using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> AddSchedule(Schedule schedule)
        {
            try
            {
                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при добавлении расписания: {ex.Message}");
            }
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
    }
}
