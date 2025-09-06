namespace Fracto.Api.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        // Optional: if not provided, we’ll default to "User" in the controller
        public string? Role { get; set; }
    }
}
