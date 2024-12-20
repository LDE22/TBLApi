using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

[Route("api/[controller]")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddService([FromBody] ServiceModel service)
    {
        var specialist = await _context.Users.FindAsync(service.SpecialistId);
        if (specialist == null)
        {
            return BadRequest(new { message = "Specialist not found." });
        }

        // Привязка города к услуге
        service.City = specialist.City;

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Service added successfully." });
    }

    [HttpPut("update-service/{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceModel updatedService)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound(new { message = "Service not found." });

        service.SpecialistName = updatedService.SpecialistName ?? service.SpecialistName;
        service.Description = updatedService.Description ?? service.Description;
        service.Price = updatedService.Price != 0 ? updatedService.Price : service.Price;

        _context.Services.Update(service);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Service updated successfully." });
    }
    [HttpGet("{specialistId}")]
    public async Task<IActionResult> GetSchedule(int specialistId)
    {
        var schedule = await _context.Schedules
            .Where(s => s.SpecialistId == specialistId)
            .ToListAsync();

        if (!schedule.Any())
            return NotFound(new { message = "График не найден." });

        return Ok(schedule);
    }
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule updatedSchedule)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
            return NotFound(new { message = "График не найден." });

        schedule.WorkingHours = updatedSchedule.WorkingHours;
        schedule.BookedIntervals = updatedSchedule.BookedIntervals;

        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();

        return Ok(new { message = "График обновлён." });
    }
    [HttpPost("book")]
    public async Task<IActionResult> BookService([FromBody] Booking booking)
    {
        var schedule = await _context.Schedules
            .FirstOrDefaultAsync(s => s.SpecialistId == booking.SpecialistId && s.Day == booking.Day);

        if (schedule == null)
            return NotFound(new { message = "График не найден." });

        if (schedule.BookedIntervals.Contains(booking.TimeInterval))
            return BadRequest(new { message = "Выбранное время уже занято." });

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

        return Ok(new { message = "Запись успешно добавлена." });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound(new { message = "Service not found." });

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Service deleted successfully." });
    }
    [HttpGet("specialist/{specialistId}")]
    public async Task<IActionResult> GetServicesBySpecialist(int specialistId)
    {
        var services = await _context.Services
            .Where(s => s.SpecialistId == specialistId)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Description,
                s.Price,
                s.SpecialistId,
                City = _context.Users.FirstOrDefault(u => u.Id == s.SpecialistId).City,
                SpecialistName = _context.Users.FirstOrDefault(u => u.Id == s.SpecialistId).Username
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchServices(string query, string city)
    {
        Console.WriteLine($"Поиск: query='{query}', city='{city}'");

        var services = await _context.Services
            .Where(s => EF.Functions.ILike(s.Title, $"%{query}%") && s.City == city)
            .ToListAsync();

        if (!services.Any())
        {
            Console.WriteLine("Услуги не найдены.");
            return NotFound(new { message = "No services found in this city." });
        }

        Console.WriteLine($"Найдено {services.Count} услуг.");
        return Ok(services);
    }


}