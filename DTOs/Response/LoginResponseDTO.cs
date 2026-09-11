namespace GuardianPet.DTOs.Response;

public sealed record LoginResponseDTO(string Token, DateTime Expiration, long UserId);
