using GuardianPet.Enums;

namespace GuardianPet.DTOs.Response
{
    public class ConsultationResponseDTO
    {
        public long Id { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public StatusConsultationEnum Status { get; set; }
    }
}