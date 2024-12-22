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
            // Проверьте, что ClientId и ServiceId присутствуют
            if (favorite.ClientId <= 0 || favorite.ServiceId <= 0)
            {
                return BadRequest("ClientId и ServiceId должны быть указаны.");
            }

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
}
