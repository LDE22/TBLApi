using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using TBLApi.Models;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Ticket>> GetTickets()
    {
        return Ok(new List<Ticket>
        {
            new Ticket { Title = "������ #1", Description = "�������� ������ 1" },
            new Ticket { Title = "������ #2", Description = "�������� ������ 2" }
        });
    }
}
