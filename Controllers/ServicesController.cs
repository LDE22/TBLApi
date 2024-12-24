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
        service.Title = updatedService.Title ?? service.Title;
        service.Description = updatedService.Description ?? service.Description;
        service.Price = updatedService.Price != 0 ? updatedService.Price : service.Price;

        _context.Services.Update(service);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Service updated successfully." });
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
 
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        try
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Услуга не найдена." });
            }

            // Вызов функции каскадного удаления
            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT delete_service({id})");

            return Ok(new { message = "Услуга успешно удалена." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении услуги: {ex.Message}");
        }
    }
    // Получить услуги специалиста
    [HttpGet("specialist/{specialistId}")]
    public async Task<IActionResult> GetServicesBySpecialist(int specialistId)
    {
        var services = await _context.Services
            .Where(s => s.SpecialistId == specialistId)
            .ToListAsync();

        if (!services.Any())
        {
            return NotFound(new { message = "No services found for this specialist." });
        }

        return Ok(new { data = services });
    }

    [HttpGet("{serviceId}")]
    public async Task<IActionResult> GetServiceById(int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);

        if (service == null)
        {
            return NotFound(new { Message = $"Service with ID {serviceId} not found." });
        }

        return Ok(service);
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