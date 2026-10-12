using GuardianPet.DTOs.Query;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Repositories;

namespace GuardianPet.Audit;

public sealed class AuditedPetRepository(IPetRepository inner, IAuditWriter audit) : IPetRepository
{
    public Task<long> CountAsync(PetQueryDTO options) => inner.CountAsync(options);
    public Task<List<Pet>> QueryAsync(PetQueryDTO options) => inner.QueryAsync(options);
    public Task<List<Pet>> FindAllAsync() => inner.FindAllAsync();
    public Task<Pet?> FindByIdAsync(long id) => inner.FindByIdAsync(id);
    public Task<List<Pet>> SearchAsync(string? species, PetSizeEnum? petSize) => inner.SearchAsync(species, petSize);

    public async Task<Pet> CreateAsync(Pet entity)
    {
        var saved = await inner.CreateAsync(entity);
        await audit.RecordAsync("Pet", saved.Id, "Created");
        return saved;
    }

    public async Task UpdateAsync(Pet entity)
    {
        await inner.UpdateAsync(entity);
        await audit.RecordAsync("Pet", entity.Id, "Updated");
    }

    public async Task DeleteAsync(Pet entity)
    {
        await inner.DeleteAsync(entity);
        await audit.RecordAsync("Pet", entity.Id, "Deleted");
    }
}
