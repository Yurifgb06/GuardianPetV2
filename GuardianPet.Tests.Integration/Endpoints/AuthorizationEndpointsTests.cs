using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GuardianPet.DTOs.Response;
using GuardianPet.Tests.Integration.Fixtures;
using Xunit;
namespace GuardianPet.Tests.Integration.Endpoints;
[Collection("Oracle integration")]
public sealed class AuthorizationEndpointsTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/api/pets")]
    [InlineData("/api/users")]
    [InlineData("/api/veterinarians")]
    [InlineData("/api/consultations")]
    public async Task Get_SemToken_DeveRetornar401(string path)
    {
        // Arrange
        using var client = fixture.Client();
        // Act
        using var response = await client.GetAsync(path);
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(response.Headers.WwwAuthenticate, h => h.Scheme == "Bearer");
    }

    [Fact]
    public async Task GetUsers_TokenComum_DeveRetornar403()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        // Act
        using var response = await client.GetAsync("/api/users");
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_Administrador_DeveRetornar200()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        // Act
        using var response = await client.GetAsync("/api/users");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponseDTO>>();
        Assert.NotNull(users);
        Assert.Contains(users, u => u.Email == fixture.AdminEmail);
    }

    [Fact]
    public async Task GetPets_TokenAdulterado_DeveRetornar401()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
        var parts = token.Split('.');
        parts[2] = (parts[2][0] == 'A' ? "B" : "A") + parts[2][1..];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", string.Join(".", parts));
        // Act
        using var response = await client.GetAsync("/api/pets");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_NovaSenha_DeveInvalidarSenhaAnterior()
    {
        // Arrange
        using var anonymous = fixture.Client();
        var dto = ApiFixture.NewUser();
        using var created = await anonymous.PostAsJsonAsync("/api/users", dto);
        var user = await created.Content.ReadFromJsonAsync<UserResponseDTO>();
        Assert.NotNull(user);
        var oldPassword = dto.Password;
        dto.Password = "Changed-" + Guid.NewGuid().ToString("N");
        using var admin = await fixture.AuthenticatedClientAsync(admin: true);
        // Act
        using var updated = await admin.PutAsJsonAsync("/api/users/" + user.Id, dto);
        using var oldLogin = await anonymous.PostAsJsonAsync("/api/auth/login", new { dto.Email, Password = oldPassword });
        using var newLogin = await anonymous.PostAsJsonAsync("/api/auth/login", new { dto.Email, dto.Password });
        // Assert
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }
}
