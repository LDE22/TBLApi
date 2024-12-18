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
            new Ticket { Title = "Жалоба #1", Description = "Описание жалобы 1" },
            new Ticket { Title = "Жалоба #2", Description = "Описание жалобы 2" }
        });
    }
}
