using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;
using TBLApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TBLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, IEmailSender emailSender, ILogger<UsersController> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
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
                return NotFound(new { message = "No user found with the specified email." });
            }

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetExpiration = DateTime.UtcNow.AddHours(1);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var resetUrl = $"{Request.Scheme}://{Request.Host}/api/Users/reset-password/{user.PasswordResetToken}";
            await _emailSender.SendEmailAsync(user.Email, "Reset your password", $"Click <a href='{resetUrl}'>here</a> to reset your password.");

            return Ok(new { message = "Password reset email sent." });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            _logger.LogInformation("Начат процесс сброса пароля.");

            if (string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                _logger.LogWarning("Токен или новый пароль не были предоставлены.");
                return BadRequest(new { message = "Токен и новый пароль обязательны." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token && u.PasswordResetExpiration > DateTime.UtcNow);

            if (user == null)
            {
                _logger.LogWarning("Неверный или истёкший токен для сброса пароля. Токен: {Token}", model.Token);
                return BadRequest(new { message = "Неверный или истёкший токен." });
            }

            try
            {
                // Хэшируем новый пароль
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetExpiration = null;

                _logger.LogInformation("Пароль для пользователя с ID {UserId} успешно обновлён.", user.Id);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Изменения сохранены в базе данных.");
                return Ok(new { message = "Пароль успешно сброшен." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сбросе пароля для пользователя с токеном: {Token}", model.Token);
                return StatusCode(500, new { message = "Произошла ошибка при сбросе пароля. Пожалуйста, попробуйте позже." });
            }
        }

        [HttpGet("reset-password/{token}")]
        public IActionResult ResetPassword(string token)
        {
            // Генерация ссылки для приложения
            var deepLinkUrl = $"myapp://reset-password?token={token}";

            // Перенаправляем пользователя в приложение
            return Redirect(deepLinkUrl);
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
        [HttpGet("Specialists/{specialistId}")]
        public async Task<IActionResult> GetSpecialistById(int specialistId)
        {
            var specialist = await _context.Users // Или другая таблица
                .FirstOrDefaultAsync(u => u.Id == specialistId && u.Role == "Specialist");

            if (specialist == null)
            {
                return NotFound(new { message = "Specialist not found." });
            }

            return Ok(specialist);
        }

    }

    public class ResetPasswordDto
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
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
    public class UpdatePasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
