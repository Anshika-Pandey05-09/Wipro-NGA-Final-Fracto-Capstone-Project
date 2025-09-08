using Fracto.Api.Data;
using Fracto.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<AppUser> _hasher;

        public AdminUsersController(ApplicationDbContext db, IPasswordHasher<AppUser> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.Username, u.Email, u.Role })
                .ToListAsync(ct);
            return Ok(users);
        }

        public class CreateUserDto
        {
            public string Username { get; set; } = "";
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
            public string Role { get; set; } = "User";
        }

        public class UpdateUserDto
        {
            public string Username { get; set; } = "";
            public string Email { get; set; } = "";
            public string? Password { get; set; }
            public string Role { get; set; } = "User";
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Username)) return BadRequest("Username is required.");
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");
            if (string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("Password is required.");

            dto.Username = dto.Username.Trim();
            dto.Email = dto.Email.Trim();

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username, ct))
                return Conflict("Username already exists.");
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email, ct))
                return Conflict("Email already exists.");

            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _hasher.HashPassword(null!, dto.Password), //Here identity hash
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetAll), new { id = user.Id },
                new { user.Id, user.Username, user.Email, user.Role });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto, CancellationToken ct)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (user is null) return NotFound();

            var newUsername = (dto.Username ?? "").Trim();
            var newEmail = (dto.Email ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != user.Username)
            {
                var exists = await _db.Users.AnyAsync(u => u.Username == newUsername && u.Id != id, ct);
                if (exists) return Conflict("Username already exists.");
                user.Username = newUsername;
            }

            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email)
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == newEmail && u.Id != id, ct);
                if (exists) return Conflict("Email already exists.");
                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = _hasher.HashPassword(user, dto.Password); // Identity hash

            if (!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = dto.Role;

            await _db.SaveChangesAsync(ct);
            return Ok(new { user.Id, user.Username, user.Email, user.Role });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (user is null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
