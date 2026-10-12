using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;

namespace GuardianPet.Services;

public interface IAuthService
{
    Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request);
}
