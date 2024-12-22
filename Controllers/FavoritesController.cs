using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetFavorites(int clientId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.ClientId == clientId)
                .ToListAsync();

            return Ok(favorites);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddToFavorite([FromBody] Favorite favorite)
        {
            if (favorite.ClientId <= 0 || favorite.ServiceId <= 0)
            {
                return BadRequest("ClientId and ServiceId are required.");
            }

            // Проверка существования клиента и услуги
            var existingClient = await _context.Users.FindAsync(favorite.ClientId);
            var existingService = await _context.Services.FindAsync(favorite.ServiceId);

            if (existingClient == null || existingService == null)
            {
                return NotFound("Client or Service not found.");
            }
            var existingFavorite = await _context.Favorites
        .FirstOrDefaultAsync(f => f.ClientId == favorite.ClientId && f.ServiceId == favorite.ServiceId);

            if (existingFavorite != null)
            {
                // Если запись уже существует, возвращаем ошибку
                return BadRequest(new { message = "The service is already in the favorites." });
            }

            // Обнуляем навигационные свойства
            favorite.Client = null;
            favorite.Service = null;

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(favorite);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromFavorite(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null) return NotFound();

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
    public class FavoriteDto
    {
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
    }

}
