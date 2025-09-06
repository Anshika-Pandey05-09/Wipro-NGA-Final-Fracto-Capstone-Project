using System.ComponentModel.DataAnnotations;

namespace Fracto.Api.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;  // ← nav named "User"

        [Range(1, 5)]
        public int Score { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
