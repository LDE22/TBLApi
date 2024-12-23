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
    public async Task<IActionResult> CreateTicket(Ticket ticket)
    {
        try
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"������ ��� �������� ������: {ex.Message}");
        }
    }

    // �������� ��� ������ (��� �����������)
    [HttpGet]
    public async Task<IActionResult> GetTickets()
    {
        try
        {
            var tickets = await _context.Tickets
                .Select(t => new
                {
                    t.Id,
                    t.ComplainantId,
                    ComplainantName = _context.Users.FirstOrDefault(u => u.Id == t.ComplainantId).Name,
                    ComplainantPhoto = _context.Users.FirstOrDefault(u => u.Id == t.ComplainantId).PhotoBase64,
                    t.TargetId,
                    TargetName = _context.Users.FirstOrDefault(u => u.Id == t.TargetId).Name,
                    TargetPhoto = _context.Users.FirstOrDefault(u => u.Id == t.TargetId).PhotoBase64,
                    t.Comment,
                    t.Status,
                    t.ActionTaken,
                    t.CreatedAt
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

