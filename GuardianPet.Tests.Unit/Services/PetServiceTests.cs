using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Repositories;
using GuardianPet.Services;
using GuardianPet.Tests.Unit.Fixtures;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
namespace GuardianPet.Tests.Unit.Services;
public sealed class PetServiceTests
{
    private readonly Mock<IPetRepository> _repository = new();
    private PetService Service() => new(_repository.Object);

    [Fact]
    public async Task FindByIdAsync_RecursoInexistente_DeveRetornar404()
    {
        // Arrange
        _repository.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Pet?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().FindByIdAsync(7));
        // Assert
        Assert.Equal(404, error.StatusCode);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Pet>()), Times.Never);
        _repository.Verify(r => r.DeleteAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RecursoInexistente_DeveRetornar404()
    {
        // Arrange
        _repository.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Pet?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().UpdateAsync(7, PetFixture.Request()));
        // Assert
        Assert.Equal(404, error.StatusCode);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Pet>()), Times.Never);
        _repository.Verify(r => r.DeleteAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RecursoInexistente_DeveRetornar404()
    {
        // Arrange
        _repository.Setup(r => r.FindByIdAsync(7)).ReturnsAsync((Pet?)null);
        // Act
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().DeleteAsync(7));
        // Assert
        Assert.Equal(404, error.StatusCode);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Pet>()), Times.Never);
        _repository.Verify(r => r.DeleteAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task FindByIdAsync_RecursoExistente_DeveMapearResposta()
    {
        // Arrange
        var entity = PetFixture.Entity();
        _repository.Setup(r => r.FindByIdAsync(entity.Id)).ReturnsAsync(entity);
        // Act
        var response = await Service().FindByIdAsync(entity.Id);
        // Assert
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.Name, response.Name);
    }

    [Fact]
    public async Task DeleteAsync_RecursoExistente_DeveExcluirEntidadeEncontrada()
    {
        // Arrange
        var entity = PetFixture.Entity();
        _repository.Setup(r => r.FindByIdAsync(entity.Id)).ReturnsAsync(entity);
        // Act
        await Service().DeleteAsync(entity.Id);
        // Assert
        _repository.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_DevePersistirEMapear()
    {
        // Arrange
        var dto = PetFixture.Request();
        Pet? saved = null;
        _repository.Setup(r => r.CreateAsync(It.IsAny<Pet>())).ReturnsAsync((Pet value) => { saved = value; value.Id = 23; return value; });
        // Act
        var response = await Service().CreateAsync(dto);
        // Assert
        Assert.NotNull(saved);
        Assert.Equal(23, response.Id);
        Assert.Equal(dto.Name, saved.Name);
        Assert.Equal(dto.Weight, saved.Weight);
        Assert.Equal(dto.PetSize, response.PetSize);
        _repository.Verify(r => r.CreateAsync(It.IsAny<Pet>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RecursoExistente_DevePersistirNovosDados()
    {
        // Arrange
        var entity = PetFixture.Entity();
        var dto = PetFixture.Request();
        dto.Name = "Nome atualizado";
        _repository.Setup(r => r.FindByIdAsync(entity.Id)).ReturnsAsync(entity);
        // Act
        var response = await Service().UpdateAsync(entity.Id, dto);
        // Assert
        Assert.Equal(dto.Name, response.Name);
        Assert.Equal(dto.Name, entity.Name);
        
        _repository.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task FindAllAsync_FalhaNoRepository_DevePropagarExcecao()
    {
        // Arrange
        var expected = new InvalidOperationException("dependency failure");
        _repository.Setup(r => r.FindAllAsync()).ThrowsAsync(expected);
        // Act
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => Service().FindAllAsync());
        // Assert
        Assert.Same(expected, actual);
    }
}

