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

        // Получить график специалиста
        [HttpGet("{specialistId}")]
        public async Task<IActionResult> GetSchedule(int specialistId)
        {
            var schedule = await _context.Schedules
                .Where(s => s.SpecialistId == specialistId)
                .ToListAsync();

            if (!schedule.Any())
                return NotFound(new { message = "Schedule not found." });

            return Ok(schedule);
        }

        // Обновить график
        [HttpPut("update-schedule/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule updatedSchedule)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound(new { message = "График не найден." });

            schedule.WorkingHours = updatedSchedule.WorkingHours;
            schedule.BookedIntervals = updatedSchedule.BookedIntervals;

            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "График обновлён." });
        }

        // Записать клиента на услугу
        [HttpPost("book")]
        public async Task<IActionResult> BookService([FromBody] Booking booking)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.SpecialistId == booking.SpecialistId && s.Day == booking.Day);

            if (schedule == null)
                return NotFound(new { message = "Schedule not found." });

            if (schedule.BookedIntervals.Contains(booking.TimeInterval))
                return BadRequest(new { message = "Time already booked." });

            schedule.BookedIntervals.Add(booking.TimeInterval);

            var appointment = new Appointment
            {
                SpecialistId = booking.SpecialistId,
                ClientId = booking.ClientId,
                ServiceId = booking.ServiceId,
                Day = booking.Day,
                TimeInterval = booking.TimeInterval
            };

            _context.Appointments.Add(appointment);
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking successful." });
        }
    }
}
