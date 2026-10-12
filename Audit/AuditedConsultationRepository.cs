using GuardianPet.DTOs.Query;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Repositories;

namespace GuardianPet.Audit;

public sealed class AuditedConsultationRepository(IConsultationRepository inner, IAuditWriter audit) : IConsultationRepository
{
    public Task<long> CountAsync(ConsultationQueryDTO options) => inner.CountAsync(options);
    public Task<List<Consultation>> QueryAsync(ConsultationQueryDTO options) => inner.QueryAsync(options);
    public Task<List<Consultation>> FindAllAsync() => inner.FindAllAsync();
    public Task<Consultation?> FindByIdAsync(long id) => inner.FindByIdAsync(id);

    public async Task<Consultation> CreateAsync(Consultation entity)
    {
        var saved = await inner.CreateAsync(entity);
        await audit.RecordAsync("Consultation", saved.Id, "Created");
        return saved;
    }

    public async Task UpdateAsync(Consultation entity)
    {
        await inner.UpdateAsync(entity);
        await audit.RecordAsync("Consultation", entity.Id, "Updated");
    }

    public async Task DeleteAsync(Consultation entity)
    {
        await inner.DeleteAsync(entity);
        await audit.RecordAsync("Consultation", entity.Id, "Deleted");
    }
}
