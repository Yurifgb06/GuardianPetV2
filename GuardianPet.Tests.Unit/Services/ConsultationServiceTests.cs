using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Repositories;
using GuardianPet.Services;
using GuardianPet.Tests.Unit.Fixtures;
using Moq;
using Xunit;
namespace GuardianPet.Tests.Unit.Services;
public sealed class ConsultationServiceTests
{
    private readonly Mock<IConsultationRepository> _consultations = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<IVeterinarianRepository> _vets = new();
    private ConsultationService Service() => new(_consultations.Object, _users.Object, _pets.Object, _vets.Object);
    private void ValidRelationships()
    {
        _users.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(UserFixture.Entity());
        _pets.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(PetFixture.Entity());
        _vets.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(VeterinarianFixture.Entity());
    }

    [Fact]
    public async Task CreateAsync_RelacionamentosValidos_DevePersistirConsulta()
    {
        // Arrange
        ValidRelationships();
        var dto = ConsultationFixture.Request();
        Consultation? saved = null;
        _consultations.Setup(r => r.CreateAsync(It.IsAny<Consultation>())).ReturnsAsync((Consultation c) => { saved = c; c.Id = 19; return c; });
        // Act
        var response = await Service().CreateAsync(dto);
        // Assert
        Assert.NotNull(saved);
        Assert.Equal(dto.UserId, saved.UserId);
        Assert.Equal(dto.PetId, saved.PetId);
        Assert.Equal(dto.VeterinarianId, saved.VeterinarianId);
        Assert.Equal(dto.ConsultationDate, response.ConsultationDate);
        Assert.Equal(dto.Symptoms, response.Symptoms);
        Assert.Equal(19, response.Id);
    }

    [Theory]
    [InlineData("user", "Usuário")]
    [InlineData("pet", "Pet")]
    [InlineData("vet", "Veterinário")]
    public async Task CreateAsync_RelacionamentoInexistente_DeveRecusarSemPersistir(string missing, string message)
    {
        // Arrange
        ValidRelationships();
        if (missing == "user") _users.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((User?)null);
        if (missing == "pet") _pets.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Pet?)null);
        if (missing == "vet") _vets.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Veterinarian?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().CreateAsync(ConsultationFixture.Request()));
        // Assert
        Assert.Equal(400, error.StatusCode);
        Assert.Contains(message, error.Message);
        _consultations.Verify(r => r.CreateAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task FindByIdAsync_ConsultaInexistente_DeveRetornar404()
    {
        // Arrange
        _consultations.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Consultation?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().FindByIdAsync(7));
        // Assert
        Assert.Equal(404, error.StatusCode);
        _consultations.Verify(r => r.UpdateAsync(It.IsAny<Consultation>()), Times.Never);
        _consultations.Verify(r => r.DeleteAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ConsultaInexistente_DeveRetornar404()
    {
        // Arrange
        _consultations.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Consultation?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().UpdateAsync(7, ConsultationFixture.Request()));
        // Assert
        Assert.Equal(404, error.StatusCode);
        _consultations.Verify(r => r.UpdateAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RelacionamentoInvalido_DevePreservarConsulta()
    {
        // Arrange
        var entity = ConsultationFixture.Entity();
        _consultations.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(entity);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().UpdateAsync(7, ConsultationFixture.Request()));
        // Assert
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("Sintomas antigos", entity.Symptoms);
        _consultations.Verify(r => r.UpdateAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DadosValidos_DeveAtualizarConsulta()
    {
        // Arrange
        ValidRelationships();
        var entity = ConsultationFixture.Entity();
        var dto = ConsultationFixture.Request();
        _consultations.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(entity);
        // Act
        var response = await Service().UpdateAsync(7, dto);
        // Assert
        Assert.Equal(dto.Symptoms, response.Symptoms);
        Assert.Equal(dto.Treatment, entity.Treatment);
        _consultations.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ConsultaInexistente_DeveRetornar404()
    {
        // Arrange
        _consultations.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Consultation?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().DeleteAsync(7));
        // Assert
        Assert.Equal(404, error.StatusCode);
        _consultations.Verify(r => r.DeleteAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ConsultaExistente_DeveExcluir()
    {
        // Arrange
        var entity = ConsultationFixture.Entity();
        _consultations.Setup(r => r.FindByIdAsync(7)).ReturnsAsync(entity);
        // Act
        await Service().DeleteAsync(7);
        // Assert
        _consultations.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_FalhaNaDependencia_DevePropagarSemPersistir()
    {
        // Arrange
        _users.Setup(r => r.FindByIdAsync(7)).ThrowsAsync(new InvalidOperationException("failure"));
        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().CreateAsync(ConsultationFixture.Request()));
        // Assert
        _consultations.Verify(r => r.CreateAsync(It.IsAny<Consultation>()), Times.Never);
    }
}
