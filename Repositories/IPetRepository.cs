using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public interface IPetRepository
    {
        Task<long> CountAsync(PetQueryDTO options);
        Task<List<Pet>> QueryAsync(PetQueryDTO options);
        Task<List<Pet>> FindAllAsync();
        Task<Pet?> FindByIdAsync(long id);
        Task<List<Pet>> SearchAsync(string? species, PetSizeEnum? petSize);
        Task<Pet> CreateAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(Pet pet);
    }
}