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
            return await _context.Users.ToListAsync();
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Login) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Логин (или Email) и пароль обязательны.");
            }

            // Поиск пользователя
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    (u.Username == loginDto.Login || u.Email == loginDto.Login)
                    && u.Password == loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Неверный логин (или Email) или пароль.");
            }

            // Возвращаем полный объект пользователя
            var result = new User
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                Photo = user.Photo,
                LinkToProfile = user.LinkToProfile,
                Name = user.Name,
                City = user.City,
                Role = user.Role
            };

            return Ok(result);
        }

    }
    public class LoginDto
    {
        public required string Login { get; set; } // Может быть Email или Username
        public required string Password { get; set; }
    }

}
