using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Request
{
    public class VeterinarianRequestDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Crmv { get; set; } = string.Empty;

        [Required]
        public SpecialtyEnum Specialty { get; set; }

        [Required]
        [RegularExpression(@"^\d{10,11}$")]
        public string Phone { get; set; } = string.Empty;

        [Range(1, 100)]
        public int YearsExperience { get; set; }
    }
}