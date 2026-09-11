using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Request
{
    public class PetRequestDTO
    {
        [Required(ErrorMessage = "Enter the pet's name!")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your pet's species!")]
        [StringLength(100, MinimumLength = 2)]
        public string Species { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the pet's breed!")]
        [StringLength(50, MinimumLength = 2)]
        public string Breed { get; set; } = string.Empty;

        [Required]
        public AgeTypeEnum AgeType { get; set; }

        [Required]
        [Range(0, 60)]
        public int Age { get; set; }

        [Required]
        [Range(0.1, 120.0)]
        public double Weight { get; set; }

        [Required]
        public PetSizeEnum PetSize { get; set; }
    }
}