using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Services;

public interface IConsultationService
{
    Task<ConsultationResponseDTO> CreateAsync(ConsultationRequestDTO dto);
    Task<long> CountAsync(ConsultationQueryDTO options);
    Task<List<ConsultationResponseDTO>> QueryAsync(ConsultationQueryDTO options);
    Task<List<ConsultationResponseDTO>> FindAllAsync();
    Task<ConsultationResponseDTO> FindByIdAsync(long id);
    Task<ConsultationResponseDTO> UpdateAsync(long id, ConsultationRequestDTO dto);
    Task DeleteAsync(long id);
}
