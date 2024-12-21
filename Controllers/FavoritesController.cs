using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Логика добавления в избранное
            var favorite = new Favorite
            {
                ClientId = request.ClientId,
                ServiceId = request.ServiceId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Добавлено в избранное." });
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
    public class AddFavoriteRequest
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ServiceId { get; set; }
    }

}
