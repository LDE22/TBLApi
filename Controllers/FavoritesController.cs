using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
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
}
