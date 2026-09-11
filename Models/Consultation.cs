using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuardianPet.Enums;

namespace GuardianPet.Models
{
    [Table("tb_consultations")]
    public class Consultation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        public User? User { get; set; }

        [Required]
        public long PetId { get; set; }

        public Pet? Pet { get; set; }

        [Required]
        public long VeterinarianId { get; set; }

        public Veterinarian? Veterinarian { get; set; }

        [Required]
        public DateTime ConsultationDate { get; set; }

        [Required]
        public string Symptoms { get; set; } = string.Empty;

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public string Treatment { get; set; } = string.Empty;

        [Required]
        public string Observations { get; set; } = string.Empty;

        [Required]
        public StatusConsultationEnum Status { get; set; }
    }
}