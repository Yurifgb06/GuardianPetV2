using GuardianPet.DTOs.Query;
using GuardianPet.Models;
using GuardianPet.Repositories;
using GuardianPet.Services;
using GuardianPet.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace GuardianPet.Tests.Unit.Queries;

public sealed class QueryServiceTests
{

    [Fact]
    public async Task Pet_FilteredQuery_DelegatesBeforeMapping()
    {
        var repository = new Mock<IPetRepository>();
        var options = new PetQueryDTO { Species = "Dog", PetSize = GuardianPet.Enums.PetSizeEnum.SMALL, Page = 2, PageSize = 3, SortBy = "id", SortDirection = "desc" };
        repository.Setup(r => r.QueryAsync(options)).ReturnsAsync([new Pet { Id = 42 }]);
        var result = await new PetService(repository.Object).QueryAsync(options);
        Assert.Equal(42, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(options), Times.Once);
        repository.Verify(r => r.FindAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Pet_WithoutOptions_UsesLegacyListing()
    {
        var repository = new Mock<IPetRepository>();
        repository.Setup(r => r.FindAllAsync()).ReturnsAsync([new Pet { Id = 7 }]);
        var result = await new PetService(repository.Object).QueryAsync(new PetQueryDTO());
        Assert.Equal(7, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(It.IsAny<PetQueryDTO>()), Times.Never);
    }

    [Fact]
    public async Task Pet_InvalidOptions_DoesNotCallRepository()
    {
        var repository = new Mock<IPetRepository>(MockBehavior.Strict);
        var error = await Assert.ThrowsAsync<ApiException>(() => new PetService(repository.Object).QueryAsync(new PetQueryDTO { PageSize = 101 }));
        Assert.Equal(400, error.StatusCode);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task User_FilteredQuery_DelegatesBeforeMapping()
    {
        var repository = new Mock<IUserRepository>();
        var options = new UserQueryDTO { Name = "Test", Email = "test@example.test", Page = 2, PageSize = 3, SortBy = "id", SortDirection = "desc" };
        repository.Setup(r => r.QueryAsync(options)).ReturnsAsync([new User { Id = 42 }]);
        var result = await new UserService(repository.Object, new PasswordHasher<User>()).QueryAsync(options);
        Assert.Equal(42, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(options), Times.Once);
        repository.Verify(r => r.FindAllAsync(), Times.Never);
    }

    [Fact]
    public async Task User_WithoutOptions_UsesLegacyListing()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.FindAllAsync()).ReturnsAsync([new User { Id = 7 }]);
        var result = await new UserService(repository.Object, new PasswordHasher<User>()).QueryAsync(new UserQueryDTO());
        Assert.Equal(7, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(It.IsAny<UserQueryDTO>()), Times.Never);
    }

    [Fact]
    public async Task User_InvalidOptions_DoesNotCallRepository()
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        var error = await Assert.ThrowsAsync<ApiException>(() => new UserService(repository.Object, new PasswordHasher<User>()).QueryAsync(new UserQueryDTO { PageSize = 101 }));
        Assert.Equal(400, error.StatusCode);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Veterinarian_FilteredQuery_DelegatesBeforeMapping()
    {
        var repository = new Mock<IVeterinarianRepository>();
        var options = new VeterinarianQueryDTO { Name = "Test", Specialty = GuardianPet.Enums.SpecialtyEnum.CLINICA_GERAL, Page = 2, PageSize = 3, SortBy = "id", SortDirection = "desc" };
        repository.Setup(r => r.QueryAsync(options)).ReturnsAsync([new Veterinarian { Id = 42 }]);
        var result = await new VeterinarianService(repository.Object).QueryAsync(options);
        Assert.Equal(42, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(options), Times.Once);
        repository.Verify(r => r.FindAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Veterinarian_WithoutOptions_UsesLegacyListing()
    {
        var repository = new Mock<IVeterinarianRepository>();
        repository.Setup(r => r.FindAllAsync()).ReturnsAsync([new Veterinarian { Id = 7 }]);
        var result = await new VeterinarianService(repository.Object).QueryAsync(new VeterinarianQueryDTO());
        Assert.Equal(7, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(It.IsAny<VeterinarianQueryDTO>()), Times.Never);
    }

    [Fact]
    public async Task Veterinarian_InvalidOptions_DoesNotCallRepository()
    {
        var repository = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var error = await Assert.ThrowsAsync<ApiException>(() => new VeterinarianService(repository.Object).QueryAsync(new VeterinarianQueryDTO { PageSize = 101 }));
        Assert.Equal(400, error.StatusCode);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Consultation_FilteredQuery_DelegatesBeforeMapping()
    {
        var repository = new Mock<IConsultationRepository>();
        var options = new ConsultationQueryDTO { UserId = 1, PetId = 2, VeterinarianId = 3, Status = GuardianPet.Enums.StatusConsultationEnum.SCHEDULED, Page = 2, PageSize = 3, SortBy = "id", SortDirection = "desc" };
        repository.Setup(r => r.QueryAsync(options)).ReturnsAsync([new Consultation { Id = 42 }]);
        var result = await new ConsultationService(repository.Object, Mock.Of<IUserRepository>(), Mock.Of<IPetRepository>(), Mock.Of<IVeterinarianRepository>()).QueryAsync(options);
        Assert.Equal(42, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(options), Times.Once);
        repository.Verify(r => r.FindAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Consultation_WithoutOptions_UsesLegacyListing()
    {
        var repository = new Mock<IConsultationRepository>();
        repository.Setup(r => r.FindAllAsync()).ReturnsAsync([new Consultation { Id = 7 }]);
        var result = await new ConsultationService(repository.Object, Mock.Of<IUserRepository>(), Mock.Of<IPetRepository>(), Mock.Of<IVeterinarianRepository>()).QueryAsync(new ConsultationQueryDTO());
        Assert.Equal(7, Assert.Single(result).Id);
        repository.Verify(r => r.QueryAsync(It.IsAny<ConsultationQueryDTO>()), Times.Never);
    }

    [Fact]
    public async Task Consultation_InvalidOptions_DoesNotCallRepository()
    {
        var repository = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var error = await Assert.ThrowsAsync<ApiException>(() => new ConsultationService(repository.Object, Mock.Of<IUserRepository>(), Mock.Of<IPetRepository>(), Mock.Of<IVeterinarianRepository>()).QueryAsync(new ConsultationQueryDTO { PageSize = 101 }));
        Assert.Equal(400, error.StatusCode);
        repository.VerifyNoOtherCalls();
    }
}
