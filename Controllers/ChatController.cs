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

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (request.SenderId == request.ReceiverId)
            {
                return BadRequest(new { message = "Cannot create chat with yourself." });
            }

            var existingChat = await _context.Messages
                .AnyAsync(m => (m.SenderId == request.SenderId && m.ReceiverId == request.ReceiverId) ||
                               (m.SenderId == request.ReceiverId && m.ReceiverId == request.SenderId));

            if (existingChat)
            {
                return BadRequest(new { message = "Chat already exists." });
            }

            var initialMessage = new Message
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Content = "Chat created",
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(initialMessage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chat created successfully.", chat = initialMessage });
        }

        // Отправить сообщение
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Content))
            {
                return BadRequest(new { message = "Message content is required." });
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message sent successfully.", data = message });
        }

        // Получить чат между двумя пользователями
        [HttpGet("conversation/{userId1}/{userId2}")]
        public async Task<IActionResult> GetConversation(int userId1, int userId2)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            if (!messages.Any())
            {
                return NotFound(new { message = "No conversation found." });
            }

            return Ok(new { data = messages });
        }

        // Удалить чат
        [HttpDelete("delete-chat/{userId1}/{userId2}")]
        public async Task<IActionResult> DeleteChat(int userId1, int userId2)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1))
                .ToListAsync();

            if (!messages.Any())
            {
                return NotFound(new { message = "Chat not found." });
            }

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chat deleted successfully." });
        }

        // Получить список чатов пользователя
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

            if (!chats.Any())
            {
                return NotFound(new { message = "No chats found." });
            }

            return Ok(new { data = chats });
        }
    }
    public class CreateChatRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
}
