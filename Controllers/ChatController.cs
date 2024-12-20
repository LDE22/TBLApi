using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Message sent successfully." });
        }

        [HttpGet("conversation/{userId1}/{userId2}")]
        public async Task<IActionResult> GetConversation(int userId1, int userId2)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(messages);
        }
        // Новый метод для удаления чата
        [HttpDelete("delete-chat/{userId1}/{userId2}")]
        public async Task<IActionResult> DeleteChat(int userId1, int userId2)
        {
            var messages = _context.Messages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1));

            if (!messages.Any())
            {
                return NotFound(new { message = "Chat not found." });
            }

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chat deleted successfully." });
        }
        [HttpGet("chats/{userId}")]
        public async Task<IActionResult> GetChats(int userId)
        {
            var chats = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new ChatPreview
                {
                    ChatId = g.Key,
                    Name = _context.Users.FirstOrDefault(u => u.Id == g.Key).Username,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).First().Content,
                    Timestamp = g.Max(m => m.Timestamp),
                    TargetUserId = g.Key
                })
                .ToListAsync();

            return Ok(chats);
        }
    }
}
