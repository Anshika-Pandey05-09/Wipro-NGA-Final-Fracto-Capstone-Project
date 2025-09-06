using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fracto.Api.Data;
using Fracto.Api.Models;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public UsersController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
            => Ok(await _db.Users.AsNoTracking().Select(u => new { u.Id, u.Username, u.Role }).ToListAsync(ct));

        public class UpsertUserDto { public string Username { get; set; } = ""; public string Role { get; set; } = "User"; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertUserDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Username)) return BadRequest("Username required.");
            var u = new AppUser { Username = dto.Username.Trim(), Role = dto.Role };
            _db.Users.Add(u);
            await _db.SaveChangesAsync(ct);
            return Ok(new { u.Id, u.Username, u.Role });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertUserDto dto, CancellationToken ct)
        {
            var u = await _db.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (u == null) return NotFound();
            if (!string.IsNullOrWhiteSpace(dto.Username)) u.Username = dto.Username.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Role)) u.Role = dto.Role;
            await _db.SaveChangesAsync(ct);
            return Ok(new { u.Id, u.Username, u.Role });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var u = await _db.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (u == null) return NotFound();
            _db.Users.Remove(u);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
