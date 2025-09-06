using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fracto.Api.Data;
using Fracto.Api.Models;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public RatingsController(ApplicationDbContext db) => _db = db;

        public class RateDto
        {
            public int AppointmentId { get; set; }
            public int UserId { get; set; }
            public int DoctorId { get; set; }
            public int Score { get; set; } // 1..5
            public string? Comment { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Rate([FromBody] RateDto dto, CancellationToken ct)
        {
            if (dto.Score < 1 || dto.Score > 5) return BadRequest("Score must be 1-5.");

            var appt = await _db.Appointments
                .Include(a => a.Doctor)
                .SingleOrDefaultAsync(a => a.Id == dto.AppointmentId, ct);

            if (appt == null) return NotFound("Appointment not found.");
            if (appt.UserId != dto.UserId) return Forbid("You can only rate your own appointment.");
            if (appt.DoctorId != dto.DoctorId) return BadRequest("DoctorId mismatch.");

            // allow rating only after the appointment day
            if (appt.AppointmentDate.Date > DateTime.UtcNow.Date)
                return BadRequest("You can rate after the appointment date.");

            var exists = await _db.Ratings.AnyAsync(r => r.AppointmentId == dto.AppointmentId, ct);
            if (exists) return Conflict("This appointment is already rated.");

            var rating = new Rating
            {
                AppointmentId = appt.Id,
                UserId = dto.UserId,
                DoctorId = appt.DoctorId,
                Score = dto.Score,
                Comment = dto.Comment
            };
            _db.Ratings.Add(rating);
            await _db.SaveChangesAsync(ct);

            // recompute average
            var avg = await _db.Ratings
                .Where(r => r.DoctorId == appt.DoctorId)
                .AverageAsync(r => (double)r.Score, ct);

            appt.Doctor.Rating = (decimal)Math.Round(avg, 1);
            await _db.SaveChangesAsync(ct);

            return Ok(new { message = "Rated", average = appt.Doctor.Rating });
        }

        // === Helpers used by your RatingService ===

        [HttpGet("doctor/{doctorId:int}")]
        public async Task<IActionResult> DoctorRatings(int doctorId, CancellationToken ct)
        {
            var rows = await _db.Ratings
                .AsNoTracking()
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Score,
                    r.Comment,
                    r.CreatedAt,
                    r.UserId
                })
                .ToListAsync(ct);

            return Ok(rows);
        }

        [HttpGet("doctor/{doctorId:int}/avg")]
        public async Task<IActionResult> DoctorAverage(int doctorId, CancellationToken ct)
        {
            var any = await _db.Ratings.AnyAsync(r => r.DoctorId == doctorId, ct);
            var avg = any
                ? await _db.Ratings.Where(r => r.DoctorId == doctorId).AverageAsync(r => (double)r.Score, ct)
                : 0.0;
            var val = any ? Math.Round(avg, 1) : 0.0;
            return Ok(new { doctorId, average = val });
        }
    }
}
