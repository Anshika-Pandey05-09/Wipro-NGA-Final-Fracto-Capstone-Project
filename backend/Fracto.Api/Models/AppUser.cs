using System.ComponentModel.DataAnnotations;

namespace Fracto.Api.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;
        [Required, MaxLength(200)]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";
        public string? ProfileImagePath { get; set; }
    }
}
