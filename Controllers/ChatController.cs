using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (request.SenderId == request.ReceiverId)
            {
                return BadRequest(new { message = "Нельзя создать чат с самим собой." });
            }

            var existingChat = await _context.Chats
                .FirstOrDefaultAsync(c =>
                    (c.SenderId == request.SenderId && c.ReceiverId == request.ReceiverId) ||
                    (c.SenderId == request.ReceiverId && c.ReceiverId == request.SenderId));

            if (existingChat != null)
            {
                return Ok(existingChat);
            }

            var chat = new Chat
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                LastMessage = "Chat started",
                Timestamp = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return Ok(chat);
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null)
            {
                return NotFound(new { message = "Chat not found." });
            }

            // Удалить сообщения, связанные с чатом
            var messages = _context.Messages.Where(m => m.ChatId == chatId);
            _context.Messages.RemoveRange(messages);

            // Удалить сам чат
            _context.Chats.Remove(chat);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Chat deleted successfully." });
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetChats(int userId)
        {
            var chats = await _context.Chats
                .Where(c => c.SenderId == userId || c.ReceiverId == userId)
                .OrderByDescending(c => c.Timestamp)
                .ToListAsync();

            return Ok(chats);
        }

        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null || message.ChatId <= 0 || string.IsNullOrWhiteSpace(message.Content))
            {
                return BadRequest(new { message = "Некорректные данные сообщения." });
            }

            var chat = await _context.Chats.FindAsync(message.ChatId);
            if (chat == null)
            {
                return NotFound(new { message = "Чат не найден." });
            }

            chat.LastMessage = message.Content;
            chat.Timestamp = DateTime.UtcNow;
            _context.Messages.Add(message);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка на сервере: {ex.Message}");
            }
        }

        public class CreateChatRequest
        {
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
        }

    }
}
