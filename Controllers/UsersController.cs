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
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.Login) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest("Логин и пароль обязательны.");
                }

                // SQL-запрос с кавычками для PostgreSQL
                var user = await _context.Users
                    .FromSqlRaw(@"SELECT * 
                          FROM ""Users"" 
                          WHERE (""Username"" = {0} OR ""Email"" = {0}) 
                          AND ""Password"" = {1}", loginDto.Login, loginDto.Password)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Unauthorized("Неверный логин или пароль.");
                }

                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Photo,
                    user.Role
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сервера: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }
    public class LoginDto
    {
        public required string Login { get; set; } // Может быть Email или Username
        public required string Password { get; set; }
    }

}
