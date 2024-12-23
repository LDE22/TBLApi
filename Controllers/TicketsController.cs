using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;
using Org.BouncyCastle.Asn1.Ocsp;

[ApiController]
[Route("api/Tickets")]
public class TicketController : ControllerBase
{
    private readonly AppDbContext _context;

    public TicketController(AppDbContext context)
    {
        _context = context;
    }

    // Создать тикет
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] Ticket ticket)
    {
        if (ticket == null)
        {
            return BadRequest("Ticket is required.");
        }

        try
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating ticket: {ex.Message} | Inner Exception: {ex.InnerException?.Message}");
        }
    }

    // Получить все тикеты (для модераторов)
    [HttpGet]
    public async Task<IActionResult> GetTickets()
    {
        try
        {
            var tickets = await _context.Tickets
                .Select(t => new Ticket
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    TargetId = t.TargetId,
                    Comment = t.Comment,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    ModeratorId = t.ModeratorId,
                    ActionTaken = t.ActionTaken,
                })
                .ToListAsync();

            return Ok(tickets);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при получении тикетов: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        try
        {
            // Ищем тикет по его ID
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
            {
                return NotFound($"Тикет с ID {id} не найден.");
            }

            // Формируем расширенный ответ, если нужно
            var response = new
            {
                ticket.Id,
                ticket.UserId,
                ticket.TargetId,
                ticket.Comment,
                ticket.Status,
                ticket.CreatedAt,
                ticket.ModeratorId,
                ticket.ActionTaken,
                ComplainantName = _context.Users.FirstOrDefault(u => u.Id == ticket.UserId)?.Name,
                ComplainantPhoto = _context.Users.FirstOrDefault(u => u.Id == ticket.UserId)?.PhotoBase64,
                TargetName = _context.Users.FirstOrDefault(u => u.Id == ticket.TargetId)?.Name,
                TargetPhoto = _context.Users.FirstOrDefault(u => u.Id == ticket.TargetId)?.PhotoBase64
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при получении тикета: {ex.Message}");
        }
    }



    // Обновить тикет (действие модератора)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] Ticket updatedTicket)
    {
        try
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = updatedTicket.Status;
            ticket.ActionTaken = updatedTicket.ActionTaken;

            // Обновляем статистику модератора
            var moderatorId = updatedTicket.ModeratorId; // Используем ModeratorId из объекта updatedTicket
            var statistic = await _context.ModeratorStatistics.FirstOrDefaultAsync(m => m.ModeratorId == moderatorId);

            if (statistic == null)
            {
                statistic = new ModeratorStatistic
                {
                    ModeratorId = moderatorId
                };
                _context.ModeratorStatistics.Add(statistic);
            }

            switch (updatedTicket.ActionTaken)
            {
                case "Заблокировать":
                    statistic.BlockedProfiles++;
                    break;
                case "Ограничение":
                    statistic.RestrictedProfiles++;
                    break;
            }

            if (updatedTicket.Status == "Отклонено")
            {
                statistic.RejectedTickets++;
            }
            else if (updatedTicket.Status == "Закрыто")
            {
                statistic.ClosedTickets++;
            }

            await _context.SaveChangesAsync();
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при обновлении тикета: {ex.Message}");
        }
    }

    // Удалить тикет
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        try
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении тикета: {ex.Message}");
        }
    }
}

