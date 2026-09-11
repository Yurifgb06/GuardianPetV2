using GuardianPet.Enums;

namespace GuardianPet.DTOs.Response
{
    public class PetResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public AgeTypeEnum AgeType { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public PetSizeEnum PetSize { get; set; }
    }
}