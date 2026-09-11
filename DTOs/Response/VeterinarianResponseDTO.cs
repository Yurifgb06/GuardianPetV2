using GuardianPet.Enums;

namespace GuardianPet.DTOs.Response
{
    public class VeterinarianResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Crmv { get; set; } = string.Empty;
        public SpecialtyEnum Specialty { get; set; }
        public string Phone { get; set; } = string.Empty;
        public int YearsExperience { get; set; }
    }
}