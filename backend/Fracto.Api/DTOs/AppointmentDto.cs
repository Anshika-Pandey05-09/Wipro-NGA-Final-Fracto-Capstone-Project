namespace Fracto.Api.DTOs
{
    public class AppointmentDto
    {
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = null!;
    }
}
