using GuardianPet.DTOs.Request;
using GuardianPet.Models;
using GuardianPet.Enums;
namespace GuardianPet.Tests.Unit.Fixtures;
public static class ConsultationFixture
{
    public static ConsultationRequestDTO Request() => new() { UserId = 7, PetId = 7, VeterinarianId = 7, ConsultationDate = new DateTime(2026, 10, 1), Symptoms = "Sintomas teste", Diagnosis = "Diagnostico teste", Treatment = "Tratamento teste", Observations = "Observacoes teste", Status = StatusConsultationEnum.SCHEDULED };
    public static Consultation Entity() => new() { Id = 7, UserId = 7, PetId = 7, VeterinarianId = 7, Symptoms = "Sintomas antigos" };
}
