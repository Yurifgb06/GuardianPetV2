using System.Net;
using System.Net.Http.Json;
using GuardianPet.DTOs.Response;
using GuardianPet.Tests.Integration.Fixtures;
using Xunit;
namespace GuardianPet.Tests.Integration.Endpoints;
[Collection("Oracle integration")]
public sealed class PetEndpointsTests(ApiFixture fixture)
{
    [Fact]
    public async Task GetPets_Autenticado_DeveRetornar200()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        // Act
        using var response = await client.GetAsync("/api/pets");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(await response.Content.ReadFromJsonAsync<List<PetResponseDTO>>(ApiFixture.JsonOptions));
    }

    [Fact]
    public async Task GetPet_IdInexistente_DeveRetornar404()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        // Act
        using var response = await client.GetAsync("/api/pets/-1");
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<GuardianPet.Exceptions.ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Pet não encontrado", error.Message);
    }

    [Fact]
    public async Task PostPet_DadosValidos_DeveRetornar201EPermitirConsulta()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        var dto = ApiFixture.NewPet();
        // Act
        using var response = await client.PostAsJsonAsync("/api/pets", dto);
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var pet = await response.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions);
        Assert.NotNull(pet);
        Assert.True(pet.Id > 0);
        Assert.Equal(dto.Name, pet.Name);
        Assert.Equal(dto.Weight, pet.Weight);
        using var read = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(pet.Id, (await read.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions))!.Id);
    }

    [Theory]
    [InlineData("", 20)]
    [InlineData("Valid pet", -1)]
    public async Task PostPet_DadosInvalidos_DeveRetornar400(string name, double weight)
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        var dto = ApiFixture.NewPet();
        dto.Name = name;
        dto.Weight = weight;
        // Act
        using var response = await client.PostAsJsonAsync("/api/pets", dto);
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePet_DadosValidos_DevePersistirAlteracao()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        var dto = ApiFixture.NewPet();
        using var created = await client.PostAsJsonAsync("/api/pets", dto);
        var pet = (await created.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions))!;
        dto.Name = "Pet atualizado";
        // Act
        using var updated = await client.PutAsJsonAsync("/api/pets/" + pet.Id, dto);
        using var read = await client.GetAsync("/api/pets/" + pet.Id);
        // Assert
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(dto.Name, (await read.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions))!.Name);
    }

    [Fact]
    public async Task DeletePet_RecursoCriadoNoTeste_DeveRetornar204ERemover()
    {
        // Arrange
        using var client = await fixture.AuthenticatedClientAsync();
        using var created = await client.PostAsJsonAsync("/api/pets", ApiFixture.NewPet());
        var pet = (await created.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions))!;
        // Act
        using var deleted = await client.DeleteAsync("/api/pets/" + pet.Id);
        using var read = await client.GetAsync("/api/pets/" + pet.Id);
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
    }
}
