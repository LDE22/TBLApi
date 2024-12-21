using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Models;

namespace TBLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // Добавить отзыв
        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] Review review)
        {
            if (string.IsNullOrWhiteSpace(review.Content))
            {
                return BadRequest(new { message = "Review content is required." });
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review added successfully.", data = review });
        }

        // Получить отзывы специалиста
        [HttpGet("specialist/{specialistId}")]
        public async Task<IActionResult> GetReviews(int specialistId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.SpecialistId == specialistId)
                .ToListAsync();

            return Ok(new { data = reviews });
        }
    }

}
