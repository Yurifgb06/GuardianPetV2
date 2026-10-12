using GuardianPet.Models;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public interface IConsultationRepository
    {
        Task<long> CountAsync(ConsultationQueryDTO options);
        Task<List<Consultation>> QueryAsync(ConsultationQueryDTO options);
        Task<List<Consultation>> FindAllAsync();
        Task<Consultation?> FindByIdAsync(long id);
        Task<Consultation> CreateAsync(Consultation consultation);
        Task UpdateAsync(Consultation consultation);
        Task DeleteAsync(Consultation consultation);
    }
}