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

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateSchedule([FromBody] Schedule schedule)
        {
            try
            {
                // Преобразуем `Day` из `DateOnly` в `DateTime` для работы с базой данных
                var scheduleDate = schedule.Day;

                var existingSchedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.SpecialistId == schedule.SpecialistId && s.Day == scheduleDate);

                if (existingSchedule != null)
                {
                    // Обновляем существующее расписание
                    existingSchedule.StartTime = schedule.StartTime;
                    existingSchedule.EndTime = schedule.EndTime;
                    existingSchedule.BreakDuration = schedule.BreakDuration;
                }
                else
                {
                    // Добавляем новое расписание
                    var newSchedule = new Schedule
                    {
                        SpecialistId = schedule.SpecialistId,
                        Day = scheduleDate, // Сохраняем дату как `DateTime`
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        BreakDuration = schedule.BreakDuration,
                        BookedIntervals = schedule.BookedIntervals // Если используется JSON-строка
                    };
                    _context.Schedules.Add(newSchedule);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Расписание успешно добавлено или обновлено." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при добавлении или обновлении расписания: {ex.Message}");
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
