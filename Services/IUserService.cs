using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Services;

public interface IUserService
{
    Task<UserResponseDTO> CreateAsync(UserRequestDTO dto);
    Task<long> CountAsync(UserQueryDTO options);
    Task<List<UserResponseDTO>> QueryAsync(UserQueryDTO options);
    Task<List<UserResponseDTO>> FindAllAsync();
    Task<UserResponseDTO> FindByIdAsync(long id);
    Task<UserResponseDTO> FindByEmailAsync(string email);
    Task<List<UserResponseDTO>> SearchByNameAsync(string name);
    Task<UserResponseDTO> UpdateAsync(long id, UserRequestDTO dto);
    Task DeleteAsync(long id);
}
