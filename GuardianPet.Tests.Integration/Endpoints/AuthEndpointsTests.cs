using System.Net;
using System.Net.Http.Json;
using GuardianPet.Data;
using GuardianPet.DTOs.Response;
using GuardianPet.Models;
using GuardianPet.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace GuardianPet.Tests.Integration.Endpoints;
[Collection("Oracle integration")]
public sealed class AuthEndpointsTests(ApiFixture fixture)
{
    [Fact]
    public async Task Login_CredenciaisValidas_DeveRetornarTokenEUsuario()
    {
        // Arrange
        using var client = fixture.Client();
        var dto = ApiFixture.NewUser();
        using var created = await client.PostAsJsonAsync("/api/users", dto);
        var user = await created.Content.ReadFromJsonAsync<UserResponseDTO>();
        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { dto.Email, dto.Password });
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
        Assert.NotNull(login);
        Assert.NotNull(user);
        Assert.Equal(user.Id, login.UserId);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        Assert.True(login.Expiration > DateTime.UtcNow);
        Assert.DoesNotContain(dto.Password, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_SenhaIncorreta_DeveRetornar401()
    {
        // Arrange
        using var client = fixture.Client();
        var dto = ApiFixture.NewUser();
        using var created = await client.PostAsJsonAsync("/api/users", dto);
        created.EnsureSuccessStatusCode();
        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { dto.Email, Password = "Wrong-password" });
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UsuarioInexistente_DeveRetornar401()
    {
        // Arrange
        using var client = fixture.Client();
        var dto = ApiFixture.NewUser();
        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { dto.Email, dto.Password });
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("", "valid-password")]
    [InlineData("invalid-email", "valid-password")]
    [InlineData("valid@example.test", "")]
    public async Task Login_PayloadInvalido_DeveRetornar400(string email, string password)
    {
        // Arrange
        using var client = fixture.Client();
        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cadastro_UsuarioValido_DeveRetornar201EPersistirHash()
    {
        // Arrange
        using var client = fixture.Client();
        var dto = ApiFixture.NewUser();
        // Act
        using var response = await client.PostAsJsonAsync("/api/users", dto);
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponseDTO>();
        Assert.NotNull(user);
        Assert.EndsWith("/api/users/" + user.Id, response.Headers.Location!.ToString());
        using var scope = fixture.Factory.Services.CreateScope();
        var stored = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.SingleAsync(u => u.Id == user.Id);
        Assert.NotEqual(dto.Password, stored.Password);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<User>().VerifyHashedPassword(stored, stored.Password, dto.Password));
    }

    [Fact]
    public async Task Cadastro_EmailDuplicado_DeveRetornar400()
    {
        // Arrange
        using var client = fixture.Client();
        var dto = ApiFixture.NewUser();
        using var created = await client.PostAsJsonAsync("/api/users", dto);
        created.EnsureSuccessStatusCode();
        // Act
        using var response = await client.PostAsJsonAsync("/api/users", dto);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Email", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_SenhaLegada_DeveRecusarTextoPuro()
    {
        // Arrange
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dto = ApiFixture.NewUser();
        db.Users.Add(new User { Name = dto.Name, Email = dto.Email, Password = dto.Password, Cpf = dto.Cpf, Phone = dto.Phone });
        await db.SaveChangesAsync();
        using var client = fixture.Client();
        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { dto.Email, dto.Password });
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
