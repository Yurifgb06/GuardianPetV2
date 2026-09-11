using System.ComponentModel.DataAnnotations;

namespace GuardianPet.DTOs.Request;

public sealed class LoginRequestDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Password { get; set; } = string.Empty;
}
