using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // Добавление нового отзыва
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] Review review)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Отзыв успешно добавлен." });
        }
        // Получение отзывов по специалисту
        [HttpGet("{specialistId}")]
        public async Task<IActionResult> GetReviewsBySpecialist(int specialistId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.SpecialistId == specialistId)
                .ToListAsync();
            return Ok(reviews);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound(new { message = "Отзыв не найден." });

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Отзыв успешно удален." });
        }
    }
}
