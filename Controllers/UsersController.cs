using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                Console.WriteLine($"[DEBUG API] Username: {user.Username}, Role: {user.Role}");
            }

            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Обновляем только переданные поля
            if (!string.IsNullOrEmpty(updatedUser.Username))
                user.Username = updatedUser.Username;

            if (!string.IsNullOrEmpty(updatedUser.Password))
                user.Password = updatedUser.Password;

            if (!string.IsNullOrEmpty(updatedUser.Email))
                user.Email = updatedUser.Email;

            if (!string.IsNullOrEmpty(updatedUser.PhotoBase64))
                user.PhotoBase64 = updatedUser.PhotoBase64;

            if (!string.IsNullOrEmpty(updatedUser.Role))
                user.Role = updatedUser.Role;

            if (!string.IsNullOrEmpty(updatedUser.Description))
                user.Description = updatedUser.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("update-avatar/{id}")]
        public async Task<IActionResult> UpdateAvatar(int id, [FromBody] PhotoUploadRequest request)
        {
            Console.WriteLine($"[INFO] Вызван метод UpdateAvatar для пользователя с ID: {id}");

            if (string.IsNullOrEmpty(request.PhotoBase64))
            {
                Console.WriteLine("[ERROR] Base64 строка пуста.");
                return BadRequest("Base64 строка не может быть пустой.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                Console.WriteLine($"[ERROR] Пользователь с ID {id} не найден.");
                return NotFound("Пользователь не найден.");
            }

            user.PhotoBase64 = request.PhotoBase64;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[INFO] Аватарка пользователя с ID {id} обновлена.");
            return Ok(new { Message = "Аватарка успешно обновлена." });
        }


        public class PhotoUploadRequest
        {
            public string PhotoBase64 { get; set; }
        }


        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Login) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Логин и пароль обязательны.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == loginDto.Login || u.Email == loginDto.Login)
                                          && u.Password == loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Неверный логин или пароль.");
            }

            var response = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.PhotoBase64,
                user.Role
            };

            Console.WriteLine($"Ответ сервера: {System.Text.Json.JsonSerializer.Serialize(response)}");
            return Ok(response);
        }
    }
    public class LoginDto
    {
        public required string Login { get; set; } // Может быть Email или Username
        public required string Password { get; set; }
    }
    public static class AvatarDefaults
    {
        public static string DefaultAvatarBase64 => "data:image/png;base64,iVBORw0KGgoAAA...";
    }

}
