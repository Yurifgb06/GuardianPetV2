using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Enums;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Services;

public interface IVeterinarianService
{
    Task<VeterinarianResponseDTO> CreateAsync(VeterinarianRequestDTO dto);
    Task<long> CountAsync(VeterinarianQueryDTO options);
    Task<List<VeterinarianResponseDTO>> QueryAsync(VeterinarianQueryDTO options);
    Task<List<VeterinarianResponseDTO>> FindAllAsync();
    Task<VeterinarianResponseDTO> FindByIdAsync(long id);
    Task<List<VeterinarianResponseDTO>> SearchByNameAsync(string name);
    Task<List<VeterinarianResponseDTO>> SearchBySpecialtyAsync(SpecialtyEnum specialty);
    Task<VeterinarianResponseDTO> UpdateAsync(long id, VeterinarianRequestDTO dto);
    Task DeleteAsync(long id);
}
