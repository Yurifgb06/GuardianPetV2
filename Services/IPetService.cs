using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Enums;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Services;

public interface IPetService
{
    Task<PetResponseDTO> CreateAsync(PetRequestDTO dto);
    Task<long> CountAsync(PetQueryDTO options);
    Task<List<PetResponseDTO>> QueryAsync(PetQueryDTO options);
    Task<List<PetResponseDTO>> FindAllAsync();
    Task<PetResponseDTO> FindByIdAsync(long id);
    Task<List<PetResponseDTO>> SearchAsync(string? species, PetSizeEnum? petSize);
    Task<PetResponseDTO> UpdateAsync(long id, PetRequestDTO dto);
    Task DeleteAsync(long id);
}
