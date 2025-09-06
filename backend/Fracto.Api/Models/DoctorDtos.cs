using System.ComponentModel.DataAnnotations;

namespace Fracto.Api.Models
{
    public class DoctorCreateDto
    {
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string City { get; set; } = string.Empty;
        [Required] public int SpecializationId { get; set; }
        [Range(typeof(decimal), "0", "5")] public decimal Rating { get; set; } = 0m;
        [Required] public TimeSpan StartTime { get; set; }
        [Required] public TimeSpan EndTime { get; set; }
        [Range(5, 480)] public int SlotDurationMinutes { get; set; } = 30;
        [Required, MaxLength(300)] public string ProfileImagePath { get; set; } = "default.png";
    }

    public class DoctorUpdateDto : DoctorCreateDto { }
}
