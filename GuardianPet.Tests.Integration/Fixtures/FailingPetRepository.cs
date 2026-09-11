using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Repositories;
namespace GuardianPet.Tests.Integration.Fixtures;
// Used only by a derived test host. No database outage or production endpoint.
public sealed class FailingPetRepository : IPetRepository
{
    public Task<List<Pet>> FindAllAsync() => throw new InvalidOperationException("Controlled repository failure");
    public Task<Pet?> FindByIdAsync(long id) => throw new NotSupportedException();
    public Task<List<Pet>> SearchAsync(string? species, PetSizeEnum? size) => throw new NotSupportedException();
    public Task<Pet> CreateAsync(Pet pet) => throw new NotSupportedException();
    public Task UpdateAsync(Pet pet) => throw new NotSupportedException();
    public Task DeleteAsync(Pet pet) => throw new NotSupportedException();
}
