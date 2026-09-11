using GuardianPet.DTOs.Request;
using GuardianPet.Models;
using GuardianPet.Enums;
namespace GuardianPet.Tests.Unit.Fixtures;
public static class VeterinarianFixture
{
    public static VeterinarianRequestDTO Request() => new() { Name = "Veterinario Teste", Email = "vet@example.test", Crmv = "CRMV-TEST", Phone = "11900000000", YearsExperience = 5, Specialty = SpecialtyEnum.CARDIOLOGIA };
    public static Veterinarian Entity() => new() { Id = 7, Name = "Veterinario Teste", Email = "vet@example.test", Crmv = "CRMV-TEST", Phone = "11900000000", YearsExperience = 5, Specialty = SpecialtyEnum.CARDIOLOGIA };
}
