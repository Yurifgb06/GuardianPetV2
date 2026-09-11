using GuardianPet.Models;

namespace GuardianPet.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> FindAllAsync();
        Task<User?> FindByIdAsync(long id);
        Task<User?> FindByEmailAsync(string email);
        Task<List<User>> SearchByNameAsync(string name);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByCpfAsync(string cpf);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}