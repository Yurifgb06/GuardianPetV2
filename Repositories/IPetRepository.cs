using GuardianPet.Enums;
using GuardianPet.Models;

namespace GuardianPet.Repositories
{
    public interface IPetRepository
    {
        Task<List<Pet>> FindAllAsync();
        Task<Pet?> FindByIdAsync(long id);
        Task<List<Pet>> SearchAsync(string? species, PetSizeEnum? petSize);
        Task<Pet> CreateAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(Pet pet);
    }
}