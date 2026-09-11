using System.ComponentModel.DataAnnotations;

namespace GuardianPet.DTOs.Request
{
    public class UserRequestDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Invalid CPF")]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10,11}$")]
        public string Phone { get; set; } = string.Empty;
    }
}