using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public interface IVeterinarianRepository
    {
        Task<long> CountAsync(VeterinarianQueryDTO options);
        Task<List<Veterinarian>> QueryAsync(VeterinarianQueryDTO options);
        Task<List<Veterinarian>> FindAllAsync();
        Task<Veterinarian?> FindByIdAsync(long id);
        Task<List<Veterinarian>> SearchByNameAsync(string name);
        Task<List<Veterinarian>> SearchBySpecialtyAsync(SpecialtyEnum specialty);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByCrmvAsync(string crmv);
        Task<Veterinarian> CreateAsync(Veterinarian veterinarian);
        Task UpdateAsync(Veterinarian veterinarian);
        Task DeleteAsync(Veterinarian veterinarian);
    }
}