using System.Net;
using System.Net.Http.Json;
using GuardianPet.Exceptions;
using GuardianPet.Repositories;
using GuardianPet.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
namespace GuardianPet.Tests.Integration.Endpoints;
[Collection("Oracle integration")]
public sealed class InfrastructureEndpointsTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    public async Task Health_SemAutenticacao_DeveRetornarHealthy(string path)
    {
        // Arrange
        using var client = fixture.Client();
        // Act
        using var response = await client.GetAsync(path);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/health", 200)]
    [InlineData("/api/pets/-1", 404)]
    public async Task Get_CorrelationIdFornecido_DevePreservarHeader(string path, int status)
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        var id = "integration-" + Guid.NewGuid().ToString("N");
        client.DefaultRequestHeaders.Add("X-Correlation-ID", id);
        // Act
        using var response = await client.GetAsync(path);
        // Assert
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal(id, Assert.Single(response.Headers.GetValues("X-Correlation-ID")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid id")]
    public async Task Get_CorrelationIdAusenteOuInvalido_DeveGerarGuid(string? id)
    {
        // Arrange
        using var client = fixture.Client();
        if (id is not null) client.DefaultRequestHeaders.Add("X-Correlation-ID", id);
        // Act
        using var response = await client.GetAsync("/health");
        // Assert
        Assert.True(Guid.TryParse(Assert.Single(response.Headers.GetValues("X-Correlation-ID")), out _));
    }

    [Fact]
    public async Task GetPets_RepositoryLancaExcecao_DeveRetornar500ComCorrelationId()
    {
        // Arrange
        using var authenticated = await fixture.AuthenticatedClientAsync();
        await using var broken = fixture.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPetRepository>();
            services.AddScoped<IPetRepository, FailingPetRepository>();
        }));
        using var client = broken.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "controlled-failure");
        // Act
        using var response = await client.GetAsync("/api/pets");
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(500, error.StatusCode);
        Assert.Equal("Erro interno do servidor.", error.Message);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Controlled repository failure", body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("StackTrace", body);
        Assert.Equal("controlled-failure", Assert.Single(response.Headers.GetValues("X-Correlation-ID")));
    }

    [Fact]
    public async Task Swagger_SemToken_DeveExporDocumentoBearer()
    {
        // Arrange
        using var client = fixture.Client();
        // Act
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("bearer", document.RootElement.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer").GetProperty("scheme").GetString());
        var paths = document.RootElement.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        foreach (var operation in path.Value.EnumerateObject())
        {
            Assert.False(string.IsNullOrWhiteSpace(operation.Value.GetProperty("summary").GetString()));
            var anonymous = path.Name is "/api/auth/login" or "/health" or "/health/ready"
                || path.Name == "/api/users" && operation.Name == "post";
            var hasSecurity = operation.Value.TryGetProperty("security", out var security) && security.GetArrayLength() > 0;
            Assert.Equal(!anonymous, hasSecurity);
            if (hasSecurity) Assert.True(security[0].TryGetProperty("Bearer", out _));
        }
        Assert.Contains("credenciais", paths.GetProperty("/api/auth/login").GetProperty("post").GetProperty("summary").GetString());
        Assert.True(paths.GetProperty("/api/users").GetProperty("post").GetProperty("responses").TryGetProperty("201", out _));
        Assert.True(paths.GetProperty("/api/users").GetProperty("get").GetProperty("responses").TryGetProperty("403", out _));
        Assert.False(paths.GetProperty("/api/users").GetProperty("post").GetProperty("responses").TryGetProperty("401", out _));
        Assert.True(paths.GetProperty("/health/ready").GetProperty("get").GetProperty("responses").TryGetProperty("503", out _));
    }
}
