namespace Fracto.Api.DTOs
{
    public class CreateRatingDto
    {
        public int DoctorId { get; set; }
        public int Score { get; set; } // 1..5
    }
}
