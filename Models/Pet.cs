using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuardianPet.Enums;

namespace GuardianPet.Models
{
    [Table("tb_pets")]
    public class Pet
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Species { get; set; } = string.Empty;

        [Required]
        public string Breed { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        public double Weight { get; set; }

        [Required]
        public AgeTypeEnum AgeType { get; set; }

        [Required]
        public PetSizeEnum PetSize { get; set; }

        public List<User> Users { get; set; } = new();

        public List<Consultation> Consultations { get; set; } = new();
    }
}