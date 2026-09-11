using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuardianPet.Enums;

namespace GuardianPet.Models
{
    [Table("tb_veterinarians")]
    public class Veterinarian
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Crmv { get; set; } = string.Empty;

        [Required]
        public SpecialtyEnum Specialty { get; set; }

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public int YearsExperience { get; set; }

        public List<Consultation> Consultations { get; set; } = new();
    }
}