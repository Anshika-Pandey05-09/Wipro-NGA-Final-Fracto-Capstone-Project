using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fracto.Api.Data;
using Fracto.Api.DTOs;
using Fracto.Api.Models;
using Fracto.Api.Services;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<AppUser> _hasher;
        private readonly ITokenService _tokenService;

        public AuthController(
            ApplicationDbContext db,
            ITokenService tokenService,
            IPasswordHasher<AppUser> hasher)
        {
            _db = db;
            _tokenService = tokenService;
            _hasher = hasher;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Invalid payload." });

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email, ct))
                return BadRequest(new { message = "User already exists." });

            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email,
                ProfileImagePath = "default.png",
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role!
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return Ok(new { user.Id, user.Username });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username, ct);
            if (user == null) return Unauthorized();

            PasswordVerificationResult result;
            try
            {
                result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            }
            catch (FormatException)
            {
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.PasswordHash = _hasher.HashPassword(user, dto.Password);
                    await _db.SaveChangesAsync(ct);
                }
                var token = _tokenService.CreateToken(user);
                return Ok(new { token, id = user.Id, username = user.Username, role = user.Role });
            }

            return Unauthorized();
        }
    }
}
