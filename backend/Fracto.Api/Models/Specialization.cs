using System.ComponentModel.DataAnnotations;

namespace Fracto.Api.Models
{
    public class Specialization
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}
