using GuardianPet.DTOs.Request;
using GuardianPet.Models;
using GuardianPet.Enums;
namespace GuardianPet.Tests.Unit.Fixtures;
public static class PetFixture
{
    public static PetRequestDTO Request() => new() { Name = "Thor", Species = "Dog", Breed = "Labrador", Age = 3, Weight = 20, AgeType = AgeTypeEnum.ANOS, PetSize = PetSizeEnum.BIG };
    public static Pet Entity() => new() { Id = 7, Name = "Thor", Species = "Dog", Breed = "Labrador", Age = 3, Weight = 20, AgeType = AgeTypeEnum.ANOS, PetSize = PetSizeEnum.BIG };
}
