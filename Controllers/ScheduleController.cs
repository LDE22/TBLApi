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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingSchedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.SpecialistId == schedule.SpecialistId && s.Day == schedule.Day);

            if (existingSchedule != null)
            {
                existingSchedule.WorkingHours = schedule.WorkingHours;
                existingSchedule.BookedIntervals = schedule.BookedIntervals;
            }
            else
            {
                _context.Schedules.Add(schedule);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Расписание успешно обновлено." });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule updatedSchedule)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound(new { message = "Расписание не найдено." });
            }

            schedule.WorkingHours = updatedSchedule.WorkingHours;
            schedule.BreakDuration = updatedSchedule.BreakDuration;
            schedule.Day = updatedSchedule.Day;

            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Расписание обновлено." });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound(new { message = "Расписание не найдено." });
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Расписание удалено." });
        }
    }
}
