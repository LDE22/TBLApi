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
                .Select(s => new
                {
                    s.Id,
                    s.SpecialistId,
                    s.Day,
                    WorkingHours = s.WorkingHoursList,
                    BookedIntervals = s.BookedIntervalsList
                })
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
            if (schedule == null)
                return NotFound(new { message = "Schedule not found." });

            // Обновление данных
            schedule.WorkingHoursList = updatedSchedule.WorkingHoursList;
            schedule.BookedIntervalsList = updatedSchedule.BookedIntervalsList;

            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Schedule updated successfully." });
        }


        // Записать клиента на услугу
        [HttpPost("book")]
        public async Task<IActionResult> BookService([FromBody] Booking booking)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.SpecialistId == booking.SpecialistId && s.Day == DateTime.Parse(booking.Day));

            if (schedule == null)
                return NotFound(new { message = "Schedule not found." });

            // Проверка времени
            if (schedule.BookedIntervalsList.Contains(booking.TimeInterval))
                return BadRequest(new { message = "Time already booked." });

            // Добавление записи
            schedule.BookedIntervalsList.Add(booking.TimeInterval);

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
