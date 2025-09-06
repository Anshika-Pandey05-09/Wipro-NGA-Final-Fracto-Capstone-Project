using System.ComponentModel.DataAnnotations;

namespace Fracto.Api.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [Required] public int UserId { get; set; }
        public AppUser? User { get; set; }
        [Required] public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        [Required] public DateTime AppointmentDate { get; set; }
        [Required, MaxLength(50)] public string TimeSlot { get; set; } = null!;
        [Required, MaxLength(50)] public string Status { get; set; } = "Booked";
    }
}
