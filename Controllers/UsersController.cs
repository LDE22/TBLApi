using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;
using TBLApi.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BCrypt.Net;

namespace TBLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public UsersController(AppDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.EmailConfirmationToken = Guid.NewGuid().ToString();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/users/confirm-email/{user.EmailConfirmationToken}";
            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Click <a href='{confirmationLink}'>here</a> to confirm your email."
            );

            return Ok(new { message = "Registration successful. Please confirm your email." });
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.To) || string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.Body))
            {
                return BadRequest("All fields (To, Subject, Body) must be provided.");
            }

            await _emailSender.SendEmailAsync(model.To, model.Subject, model.Body);
            return Ok(new { message = "Email sent successfully." });
        }

        [HttpGet("confirm-email/{token}")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid confirmation token." });
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Email confirmed successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Login || u.Email == model.Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid login or password." });
            }

            if (!user.IsEmailConfirmed)
            {
                return Unauthorized(new { message = "Please verify your email" });
            }

            return Ok(new { user.Id, user.Username, user.Email, user.Role, user.Description, user.City, user.IsEmailConfirmed, user.PhotoBase64});
        }

        [HttpPost("send-password-reset")]
        public async Task<IActionResult> SendPasswordReset([FromBody] PasswordResetRequestModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return BadRequest(new { message = "Email not found." });
            }

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetExpiration = DateTime.UtcNow.AddHours(1);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var resetUrl = $"{Request.Scheme}://{Request.Host}/api/Users/reset-password/{user.PasswordResetToken}";
            await _emailSender.SendEmailAsync(user.Email, "Reset your password", $"Click <a href='{resetUrl}'>here</a> to reset your password.");

            return Ok(new { message = "Password reset email sent." });
        }

        [HttpPost("reset-password/{token}")]
        public async Task<IActionResult> ResetPassword(string token, [FromBody] PasswordResetModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetExpiration > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid or expired token." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiration = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }

        [HttpPut("update-avatar/{id}")]
        public async Task<IActionResult> UpdateAvatar(int id, [FromBody] PhotoUploadRequest request)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.PhotoBase64 = request.PhotoBase64;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Avatar updated successfully." });
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
        [HttpGet("logs")]
        public async Task<IActionResult> GetActionLogs()
        {
            var logs = await _context.ActionLogs
                .Include(log => log.User) // Если требуется информация о пользователе
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpGet("{id}")]
public async Task<IActionResult> GetUserById(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null)
    {
        return NotFound(new { message = "Пользователь не найден" });
    }
    return Ok(user);
}

        // Получение логов по UserId
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetLogsByUser(int userId)
        {
            var logs = await _context.ActionLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            if (!logs.Any())
            {
                return NotFound(new { message = $"No logs found for User ID {userId}." });
            }

            return Ok(logs);
        }
        [HttpPut("update-profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileModel model)
        {
            // Найти пользователя в базе данных
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Обновление данных, только если они указаны в запросе
            user.Username = !string.IsNullOrEmpty(model.Username) ? model.Username : user.Username;
            user.Password = !string.IsNullOrEmpty(model.Password) ? model.Password : user.Password;
            user.Email = !string.IsNullOrEmpty(model.Email) ? model.Email : user.Email;
            user.Role = !string.IsNullOrEmpty(model.Role) ? model.Role : user.Role;
            user.Name = !string.IsNullOrEmpty(model.Name) ? model.Name : user.Name;
            user.City = !string.IsNullOrEmpty(model.City) ? model.City : user.City;
            user.Description = !string.IsNullOrEmpty(model.Description) ? model.Description : user.Description;

            // Сохранение изменений
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully", user });
        }
    }
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] Service updatedService)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound(new { message = "Service not found." });

            service.SpecialistName = updatedService.Name ?? service.SpecialistName;
            service.Description = updatedService.Description ?? service.Description;
            service.Price = updatedService.Price != 0 ? updatedService.Price : service.Price;

            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service updated successfully." });
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
                    TargetUserId = g.Key // Указание ID пользователя
                })
                .ToListAsync();

            return Ok(chats);
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToFavorites([FromBody] Favorite favorite)
        {
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Added to favorites." });
        }

        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveFromFavorites(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null) return NotFound(new { message = "Favorite not found." });

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Removed from favorites." });
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetFavoritesByClient(int clientId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.ClientId == clientId)
                .Include(f => f.Service)
                .ToListAsync();

            return Ok(favorites);
        }
    }

    public class PhotoUploadRequest
    {
        public string PhotoBase64 { get; set; }
    }

    public class PasswordResetRequestModel
    {
        public string Email { get; set; }
    }

    public class PasswordResetModel
    {
        public string NewPassword { get; set; }
    }

    public class LoginRequestModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int SpecialistId { get; set; }
    }
}
