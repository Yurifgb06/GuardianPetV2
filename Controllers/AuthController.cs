using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Exceptions;
using GuardianPet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuardianPet.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>Valida as credenciais e emite um token JWT para acesso à API.</summary>
    /// <remarks>Acesso anônimo; não exige token JWT.</remarks>
    /// <param name="request">E-mail e senha do usuário cadastrado.</param>
    /// <returns>Token, expiração e identificador do usuário; 401 se as credenciais forem inválidas.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO request)
    {
        var response = await authService.LoginAsync(request);
        return response is null ? Unauthorized() : Ok(response);
    }
}
