using GuardianPet.DTOs.Query;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Repositories;

namespace GuardianPet.Audit;

public sealed class AuditedUserRepository(IUserRepository inner, IAuditWriter audit) : IUserRepository
{
    public Task<long> CountAsync(UserQueryDTO options) => inner.CountAsync(options);
    public Task<List<User>> QueryAsync(UserQueryDTO options) => inner.QueryAsync(options);
    public Task<List<User>> FindAllAsync() => inner.FindAllAsync();
    public Task<User?> FindByIdAsync(long id) => inner.FindByIdAsync(id);
    public Task<User?> FindByEmailAsync(string email) => inner.FindByEmailAsync(email);
    public Task<List<User>> SearchByNameAsync(string name) => inner.SearchByNameAsync(name);
    public Task<bool> ExistsByEmailAsync(string email) => inner.ExistsByEmailAsync(email);
    public Task<bool> ExistsByCpfAsync(string cpf) => inner.ExistsByCpfAsync(cpf);

    public async Task<User> CreateAsync(User entity)
    {
        var saved = await inner.CreateAsync(entity);
        await audit.RecordAsync("User", saved.Id, "Created");
        return saved;
    }

    public async Task UpdateAsync(User entity)
    {
        await inner.UpdateAsync(entity);
        await audit.RecordAsync("User", entity.Id, "Updated");
    }

    public async Task DeleteAsync(User entity)
    {
        await inner.DeleteAsync(entity);
        await audit.RecordAsync("User", entity.Id, "Deleted");
    }
}
