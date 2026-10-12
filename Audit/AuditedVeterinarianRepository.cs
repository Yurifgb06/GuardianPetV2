using GuardianPet.DTOs.Query;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Repositories;

namespace GuardianPet.Audit;

public sealed class AuditedVeterinarianRepository(IVeterinarianRepository inner, IAuditWriter audit) : IVeterinarianRepository
{
    public Task<long> CountAsync(VeterinarianQueryDTO options) => inner.CountAsync(options);
    public Task<List<Veterinarian>> QueryAsync(VeterinarianQueryDTO options) => inner.QueryAsync(options);
    public Task<List<Veterinarian>> FindAllAsync() => inner.FindAllAsync();
    public Task<Veterinarian?> FindByIdAsync(long id) => inner.FindByIdAsync(id);
    public Task<List<Veterinarian>> SearchByNameAsync(string name) => inner.SearchByNameAsync(name);
    public Task<List<Veterinarian>> SearchBySpecialtyAsync(SpecialtyEnum specialty) => inner.SearchBySpecialtyAsync(specialty);
    public Task<bool> ExistsByEmailAsync(string email) => inner.ExistsByEmailAsync(email);
    public Task<bool> ExistsByCrmvAsync(string crmv) => inner.ExistsByCrmvAsync(crmv);

    public async Task<Veterinarian> CreateAsync(Veterinarian entity)
    {
        var saved = await inner.CreateAsync(entity);
        await audit.RecordAsync("Veterinarian", saved.Id, "Created");
        return saved;
    }

    public async Task UpdateAsync(Veterinarian entity)
    {
        await inner.UpdateAsync(entity);
        await audit.RecordAsync("Veterinarian", entity.Id, "Updated");
    }

    public async Task DeleteAsync(Veterinarian entity)
    {
        await inner.DeleteAsync(entity);
        await audit.RecordAsync("Veterinarian", entity.Id, "Deleted");
    }
}
