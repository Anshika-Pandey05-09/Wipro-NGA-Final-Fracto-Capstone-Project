// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace Fracto.Api.Models
// {
//     public class Doctor
//     {
//         public int Id { get; set; }
//         [Required, MaxLength(100)]
//         public string Name { get; set; } = null!;
//         [Required, MaxLength(100)]
//         public string City { get; set; } = null!;
//         [Required]
//         public int SpecializationId { get; set; }
//         public Specialization? Specialization { get; set; }
//         [Range(0,5)]
//         public decimal Rating { get; set; } = 0;
//         [Required]
//         public TimeSpan StartTime { get; set; }
//         [Required]
//         public TimeSpan EndTime { get; set; }
//         [Required]
//         public int SlotDurationMinutes { get; set; } = 30;
//         [Required, MaxLength(300)]
//         public string ProfileImagePath { get; set; } = "default.png";
//     }
// }

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fracto.Api.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        public int SpecializationId { get; set; }
        public Specialization? Specialization { get; set; }

        [Range(typeof(decimal), "0", "5")]
        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 0m;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(5, 480)]
        public int SlotDurationMinutes { get; set; } = 30;

        [Required, MaxLength(300)]
        public string ProfileImagePath { get; set; } = "default.png";
    }
}
