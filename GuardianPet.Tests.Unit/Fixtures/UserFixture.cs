using GuardianPet.DTOs.Request;
using GuardianPet.Models;
namespace GuardianPet.Tests.Unit.Fixtures;
public static class UserFixture
{
    public static UserRequestDTO Request() => new() { Name = "Usuario Teste", Email = "user@example.test", Password = "Safe-Test-Password-42!", Cpf = "12345678901", Phone = "11900000000" };
    public static User Entity() => new() { Id = 7, Name = "Usuario Teste", Email = "user@example.test", Password = "legacy", Cpf = "12345678901", Phone = "11900000000" };
}
