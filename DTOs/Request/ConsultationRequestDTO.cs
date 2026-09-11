using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Request
{
    public class ConsultationRequestDTO
    {
        [Required]
        public DateTime ConsultationDate { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Symptoms { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Treatment { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Observations { get; set; } = string.Empty;

        [Required]
        public StatusConsultationEnum Status { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public long PetId { get; set; }

        [Required]
        public long VeterinarianId { get; set; }
    }
}