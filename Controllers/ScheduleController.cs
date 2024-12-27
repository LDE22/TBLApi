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
        public async Task<IActionResult> GetScheduleBySpecialist(int specialistId)
        {
            var schedule = await _context.Schedules
                .Where(s => s.SpecialistId == specialistId)
                .ToListAsync();
            return Ok(schedule);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddOrUpdateSchedule([FromBody] Schedule schedule)
        {
            try
            {
                // Убедитесь, что `Day` был правильно передан и преобразуйте его из строки, если необходимо
                var scheduleDate = schedule.Day;

                var existingSchedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.SpecialistId == schedule.SpecialistId && s.Day == scheduleDate);

                if (existingSchedule != null)
                {
                    existingSchedule.StartTime = schedule.StartTime;
                    existingSchedule.EndTime = schedule.EndTime;
                    existingSchedule.BreakDuration = schedule.BreakDuration;
                }
                else
                {
                    _context.Schedules.Add(schedule);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Расписание успешно добавлено или обновлено." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при добавлении или обновлении расписания: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
                return NotFound(new { message = "Расписание не найдено." });

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Расписание удалено." });
        }
        [HttpGet("specialist/{id}/available-intervals")]
        public async Task<IActionResult> GetAvailableIntervals(int id)
        {
            try
            {
                var schedule = await _context.Schedules
                    .Where(s => s.SpecialistId == id)
                    .ToListAsync();

                if (!schedule.Any())
                    return NotFound(new { message = "Расписание не найдено." });

                var availableIntervals = schedule.Select(s => new
                {
                    s.Day,
                    s.StartTime,
                    s.EndTime,
                    s.BreakDuration,
                    AvailableIntervals = s.AvailableIntervals // Доступные интервалы
                });

                return Ok(availableIntervals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении интервалов: {ex.Message}");
            }
        }

    }
}
