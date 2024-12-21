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
            {
                return NotFound(new { message = "Schedule not found." });
            }

            return Ok(new { data = schedule });
        }

        // Обновить график
        [HttpPut("update-schedule/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule updatedSchedule)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found." });
            }

            schedule.WorkingHoursList = updatedSchedule.WorkingHoursList;
            schedule.BookedIntervalsList = updatedSchedule.BookedIntervalsList;

            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Schedule updated successfully." });
        }

        // Бронирование услуги
        [HttpPost("book")]
        public async Task<IActionResult> BookService([FromBody] Booking booking)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.SpecialistId == booking.SpecialistId && s.Day == DateTime.Parse(booking.Day));

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found." });
            }

            if (schedule.BookedIntervalsList.Contains(booking.TimeInterval))
            {
                return BadRequest(new { message = "Time already booked." });
            }

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

        [HttpDelete("delete-booking/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Appointments.FindAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = "Booking not found." });
            }

            _context.Appointments.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking deleted successfully." });
        }


        [HttpGet("appointments/{specialistId}")]
        public async Task<IActionResult> GetAppointments(int specialistId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.SpecialistId == specialistId)
                .Select(a => new
                {
                    a.Id,
                    a.ClientId,
                    ClientName = _context.Users.FirstOrDefault(u => u.Id == a.ClientId).Username,
                    a.ServiceId,
                    ServiceTitle = _context.Services.FirstOrDefault(s => s.Id == a.ServiceId).Title,
                    a.Day,
                    a.TimeInterval
                })
                .ToListAsync();

            if (!appointments.Any())
            {
                return NotFound(new { message = "No appointments found." });
            }

            return Ok(new { data = appointments });
        }

    }
}
