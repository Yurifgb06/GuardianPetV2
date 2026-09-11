using GuardianPet.Models;

namespace GuardianPet.Repositories
{
    public interface IConsultationRepository
    {
        Task<List<Consultation>> FindAllAsync();
        Task<Consultation?> FindByIdAsync(long id);
        Task<Consultation> CreateAsync(Consultation consultation);
        Task UpdateAsync(Consultation consultation);
        Task DeleteAsync(Consultation consultation);
    }
}