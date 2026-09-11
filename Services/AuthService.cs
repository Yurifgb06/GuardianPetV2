using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Models;
using GuardianPet.Repositories;
using GuardianPet.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GuardianPet.Services;

public sealed class AuthService(
    IUserRepository users, IPasswordHasher<User> hasher, IOptions<JwtOptions> options)
{
    public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null) return null;

        PasswordVerificationResult verification;
        try
        {
            verification = hasher.VerifyHashedPassword(user, user.Password, request.Password);
        }
        catch (FormatException)
        {
            // Legacy plaintext is never accepted as a login credential.
            return null;
        }
        if (verification == PasswordVerificationResult.Failed) return null;
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = hasher.HashPassword(user, request.Password);
            await users.UpdateAsync(user);
        }

        var settings = options.Value;
        var now = DateTime.UtcNow;
        var expiration = now.AddMinutes(settings.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (settings.AdminUserIds.Contains(user.Id))
            claims.Add(new Claim("permission", "users.manage"));

        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            now, expiration, new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256));
        return new LoginResponseDTO(new JwtSecurityTokenHandler().WriteToken(token), expiration, user.Id);
    }
}
