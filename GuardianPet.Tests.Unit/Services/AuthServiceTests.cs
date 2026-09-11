using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GuardianPet.DTOs.Request;
using GuardianPet.Models;
using GuardianPet.Repositories;
using GuardianPet.Security;
using GuardianPet.Services;
using GuardianPet.Tests.Unit.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
namespace GuardianPet.Tests.Unit.Services;
public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly PasswordHasher<User> _hasher = new();
    private readonly JwtOptions _options = new() { Issuer = "test", Audience = "test", Key = "Test-only-key-at-least-thirty-two-bytes-long", ExpirationMinutes = 15 };
    private AuthService Service(IPasswordHasher<User>? hasher = null) => new(_users.Object, hasher ?? _hasher, Options.Create(_options));
    private User ValidUser()
    {
        var user = UserFixture.Entity();
        user.Password = _hasher.HashPassword(user, UserFixture.Request().Password);
        _users.Setup(r => r.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        return user;
    }
    private static LoginRequestDTO Request() => new() { Email = UserFixture.Request().Email, Password = UserFixture.Request().Password };

    [Fact]
    public async Task LoginAsync_CredenciaisValidas_DeveEmitirTokenAssinadoSemDadosSensiveis()
    {
        // Arrange
        var user = ValidUser();
        var before = DateTime.UtcNow;
        // Act
        var response = await Service().LoginAsync(Request());
        // Assert
        Assert.NotNull(response);
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(response.Token, new TokenValidationParameters {
            ValidIssuer = _options.Issuer, ValidAudience = _options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            ValidateIssuerSigningKey = true, ValidateLifetime = true
        }, out _);
        Assert.NotNull(principal);
        var jwt = handler.ReadJwtToken(response.Token);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.DoesNotContain(jwt.Claims, c => c.Type is "permission" or "email" or "password" or "cpf");
        Assert.InRange(response.Expiration, before.AddMinutes(15), DateTime.UtcNow.AddMinutes(15));
        Assert.Equal(user.Id, response.UserId);
    }

    [Fact]
    public async Task LoginAsync_AdministradorConfigurado_DeveEmitirPermissao()
    {
        // Arrange
        var user = ValidUser();
        _options.AdminUserIds = new[] { user.Id };
        // Act
        var response = await Service().LoginAsync(Request());
        // Assert
        Assert.NotNull(response);
        Assert.Contains(new JwtSecurityTokenHandler().ReadJwtToken(response.Token).Claims,
            c => c.Type == "permission" && c.Value == "users.manage");
    }

    [Fact]
    public async Task LoginAsync_UsuarioInexistente_DeveRecusar()
    {
        // Arrange
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        // Act
        var response = await Service().LoginAsync(Request());
        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task LoginAsync_SenhaIncorreta_DeveRecusar()
    {
        // Arrange
        ValidUser();
        var request = Request();
        request.Password = "wrong";
        // Act
        var response = await Service().LoginAsync(request);
        // Assert
        Assert.Null(response);
        _users.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("plain-text-password!")]
    [InlineData("123456")]
    [InlineData("YWJjZA==")]
    public async Task LoginAsync_SenhaLegadaOuHashInvalido_DeveRecusar(string legacy)
    {
        // Arrange
        var user = UserFixture.Entity();
        user.Password = legacy;
        _users.Setup(r => r.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        var request = Request();
        request.Password = legacy;
        // Act
        var response = await Service().LoginAsync(request);
        // Assert
        Assert.Null(response);
        _users.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_HashAntigoValido_DevePersistirRehash()
    {
        // Arrange
        var user = ValidUser();
        var hasher = new Mock<IPasswordHasher<User>>();
        hasher.Setup(h => h.VerifyHashedPassword(user, user.Password, It.IsAny<string>())).Returns(PasswordVerificationResult.SuccessRehashNeeded);
        hasher.Setup(h => h.HashPassword(user, It.IsAny<string>())).Returns("updated-hash");
        // Act
        var response = await Service(hasher.Object).LoginAsync(Request());
        // Assert
        Assert.NotNull(response);
        Assert.Equal("updated-hash", user.Password);
        _users.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_FalhaNoRepository_DevePropagar()
    {
        // Arrange
        _users.Setup(r => r.FindByEmailAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("failure"));
        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().LoginAsync(Request()));
        // Assert
        _users.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
