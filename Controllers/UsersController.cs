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
            if (id != updatedUser.Id)
                return BadRequest("ID ������������ �� ���������.");

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound("������������ �� ������.");

            // ��������� ������ ������������
            existingUser.Name = updatedUser.Name;
            existingUser.City = updatedUser.City;
            existingUser.Description = updatedUser.Description;
            existingUser.Photo = updatedUser.Photo;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("������������ �������.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"������ ���������� ������: {ex.Message}");
            }
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
                return BadRequest("����� � ������ �����������.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == loginDto.Login || u.Email == loginDto.Login)
                                          && u.Password == loginDto.Password);

            if (user == null)
            {
                return Unauthorized("�������� ����� ��� ������.");
            }

            var response = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Photo,
                user.Role
            };

            Console.WriteLine($"����� �������: {System.Text.Json.JsonSerializer.Serialize(response)}");
            return Ok(response);
        }
    }
    public class LoginDto
    {
        public required string Login { get; set; } // ����� ���� Email ��� Username
        public required string Password { get; set; }
    }

}
