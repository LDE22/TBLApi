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

    // ������� �����
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] Ticket ticket)
    {
        if (ticket == null)
        {
            return BadRequest("Ticket is required.");
        }
        ticket.ComplainantName = null;
        ticket.ComplainantPhoto = null;
        ticket.TargetName = null;
        ticket.TargetPhoto = null;

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

    // �������� ��� ������ (��� �����������)
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
                    ComplainantName = _context.Users.FirstOrDefault(u => u.Id == t.UserId).Name,
                    ComplainantPhoto = _context.Users.FirstOrDefault(u => u.Id == t.UserId).PhotoBase64,
                    TargetName = _context.Users.FirstOrDefault(u => u.Id == t.TargetId).Name,
                    TargetPhoto = _context.Users.FirstOrDefault(u => u.Id == t.TargetId).PhotoBase64
                })
                .ToListAsync();

            return Ok(tickets);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"������ ��� ��������� �������: {ex.Message}");
        }
    }


    // �������� ����� (�������� ����������)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] Ticket updatedTicket)
    {
        try
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = updatedTicket.Status;
            ticket.ActionTaken = updatedTicket.ActionTaken;

            // ��������� ���������� ����������
            var moderatorId = updatedTicket.ModeratorId; // ���������� ModeratorId �� ������� updatedTicket
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
                case "�������������":
                    statistic.BlockedProfiles++;
                    break;
                case "�����������":
                    statistic.RestrictedProfiles++;
                    break;
            }

            if (updatedTicket.Status == "���������")
            {
                statistic.RejectedTickets++;
            }
            else if (updatedTicket.Status == "�������")
            {
                statistic.ClosedTickets++;
            }

            await _context.SaveChangesAsync();
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"������ ��� ���������� ������: {ex.Message}");
        }
    }

    // ������� �����
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
            return StatusCode(500, $"������ ��� �������� ������: {ex.Message}");
        }
    }
}

