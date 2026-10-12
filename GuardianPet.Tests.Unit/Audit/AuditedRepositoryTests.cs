using GuardianPet.Audit;
using GuardianPet.Models;
using GuardianPet.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GuardianPet.Tests.Unit.Audit;

public sealed class AuditedRepositoryTests
{
    private static AuditService Writer(IAuditRepository repository)
    {
        var services = new ServiceCollection().AddSingleton(repository).BuildServiceProvider();
        return new AuditService(services, new HttpContextAccessor(), Options.Create(new MongoAuditOptions()),
            NullLogger<AuditService>.Instance);
    }

    [Fact]
    public async Task Pet_Created_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Pet { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync((Pet entity) => { calls.Add("oracle"); entity.Id = 23; return entity; });
        audit.Setup(x => x.RecordAsync("Pet", 23, "Created")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedPetRepository(inner.Object, audit.Object).CreateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Pet_Created_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Pet { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedPetRepository(inner.Object, audit.Object).CreateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Pet_Created_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Pet { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync(entity);
        await new AuditedPetRepository(inner.Object, Writer(repository.Object)).CreateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Pet" && e.EntityId == 23 && e.Action == "Created"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pet_Updated_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Pet { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.UpdateAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("Pet", 23, "Updated")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedPetRepository(inner.Object, audit.Object).UpdateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Pet_Updated_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Pet { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedPetRepository(inner.Object, audit.Object).UpdateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Pet_Updated_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Pet { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedPetRepository(inner.Object, Writer(repository.Object)).UpdateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Pet" && e.EntityId == 23 && e.Action == "Updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pet_Deleted_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Pet { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.DeleteAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("Pet", 23, "Deleted")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedPetRepository(inner.Object, audit.Object).DeleteAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Pet_Deleted_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Pet { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedPetRepository(inner.Object, audit.Object).DeleteAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Pet_Deleted_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IPetRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Pet { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedPetRepository(inner.Object, Writer(repository.Object)).DeleteAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Pet" && e.EntityId == 23 && e.Action == "Deleted"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task User_Created_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new User { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync((User entity) => { calls.Add("oracle"); entity.Id = 23; return entity; });
        audit.Setup(x => x.RecordAsync("User", 23, "Created")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedUserRepository(inner.Object, audit.Object).CreateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task User_Created_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new User { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedUserRepository(inner.Object, audit.Object).CreateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task User_Created_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new User { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync(entity);
        await new AuditedUserRepository(inner.Object, Writer(repository.Object)).CreateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "User" && e.EntityId == 23 && e.Action == "Created"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task User_Updated_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new User { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.UpdateAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("User", 23, "Updated")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedUserRepository(inner.Object, audit.Object).UpdateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task User_Updated_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new User { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedUserRepository(inner.Object, audit.Object).UpdateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task User_Updated_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new User { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedUserRepository(inner.Object, Writer(repository.Object)).UpdateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "User" && e.EntityId == 23 && e.Action == "Updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task User_Deleted_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new User { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.DeleteAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("User", 23, "Deleted")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedUserRepository(inner.Object, audit.Object).DeleteAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task User_Deleted_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new User { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedUserRepository(inner.Object, audit.Object).DeleteAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task User_Deleted_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IUserRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new User { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedUserRepository(inner.Object, Writer(repository.Object)).DeleteAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "User" && e.EntityId == 23 && e.Action == "Deleted"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Veterinarian_Created_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Veterinarian { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync((Veterinarian entity) => { calls.Add("oracle"); entity.Id = 23; return entity; });
        audit.Setup(x => x.RecordAsync("Veterinarian", 23, "Created")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedVeterinarianRepository(inner.Object, audit.Object).CreateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Veterinarian_Created_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Veterinarian { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedVeterinarianRepository(inner.Object, audit.Object).CreateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Veterinarian_Created_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Veterinarian { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync(entity);
        await new AuditedVeterinarianRepository(inner.Object, Writer(repository.Object)).CreateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Veterinarian" && e.EntityId == 23 && e.Action == "Created"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Veterinarian_Updated_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Veterinarian { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.UpdateAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("Veterinarian", 23, "Updated")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedVeterinarianRepository(inner.Object, audit.Object).UpdateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Veterinarian_Updated_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Veterinarian { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedVeterinarianRepository(inner.Object, audit.Object).UpdateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Veterinarian_Updated_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Veterinarian { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedVeterinarianRepository(inner.Object, Writer(repository.Object)).UpdateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Veterinarian" && e.EntityId == 23 && e.Action == "Updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Veterinarian_Deleted_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Veterinarian { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.DeleteAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("Veterinarian", 23, "Deleted")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedVeterinarianRepository(inner.Object, audit.Object).DeleteAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Veterinarian_Deleted_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Veterinarian { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedVeterinarianRepository(inner.Object, audit.Object).DeleteAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Veterinarian_Deleted_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Veterinarian { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedVeterinarianRepository(inner.Object, Writer(repository.Object)).DeleteAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Veterinarian" && e.EntityId == 23 && e.Action == "Deleted"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consultation_Created_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Consultation { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync((Consultation entity) => { calls.Add("oracle"); entity.Id = 23; return entity; });
        audit.Setup(x => x.RecordAsync("Consultation", 23, "Created")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedConsultationRepository(inner.Object, audit.Object).CreateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Consultation_Created_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Consultation { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedConsultationRepository(inner.Object, audit.Object).CreateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Consultation_Created_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Consultation { Id = 23 };
        inner.Setup(x => x.CreateAsync(entity)).ReturnsAsync(entity);
        await new AuditedConsultationRepository(inner.Object, Writer(repository.Object)).CreateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Consultation" && e.EntityId == 23 && e.Action == "Created"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consultation_Updated_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Consultation { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.UpdateAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("Consultation", 23, "Updated")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedConsultationRepository(inner.Object, audit.Object).UpdateAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Consultation_Updated_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Consultation { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedConsultationRepository(inner.Object, audit.Object).UpdateAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Consultation_Updated_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Consultation { Id = 23 };
        inner.Setup(x => x.UpdateAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedConsultationRepository(inner.Object, Writer(repository.Object)).UpdateAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Consultation" && e.EntityId == 23 && e.Action == "Updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consultation_Deleted_AuditsOnlyAfterOracleSuccess()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Consultation { Id = 23 };
        var calls = new List<string>();
        inner.Setup(x => x.DeleteAsync(entity)).Returns(() => { calls.Add("oracle"); return Task.CompletedTask; });
        audit.Setup(x => x.RecordAsync("Consultation", 23, "Deleted")).Returns(() => { calls.Add("audit"); return Task.CompletedTask; });
        await new AuditedConsultationRepository(inner.Object, audit.Object).DeleteAsync(entity);
        Assert.Equal(new[] { "oracle", "audit" }, calls);
        audit.VerifyAll();
    }

    [Fact]
    public async Task Consultation_Deleted_OracleFailure_DoesNotAudit()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var audit = new Mock<IAuditWriter>(MockBehavior.Strict);
        var entity = new Consultation { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).ThrowsAsync(new InvalidOperationException("Oracle failure"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AuditedConsultationRepository(inner.Object, audit.Object).DeleteAsync(entity));
        audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Consultation_Deleted_MongoFailure_PreservesCrudSuccess()
    {
        var inner = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var repository = new Mock<IAuditRepository>();
        repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal MongoDB details"));
        var entity = new Consultation { Id = 23 };
        inner.Setup(x => x.DeleteAsync(entity)).Returns(Task.CompletedTask);
        await new AuditedConsultationRepository(inner.Object, Writer(repository.Object)).DeleteAsync(entity);
        repository.Verify(x => x.InsertAsync(It.Is<AuditEvent>(e => e.EntityType == "Consultation" && e.EntityId == 23 && e.Action == "Deleted"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reads_DoNotEmitAudit()
    {
        var inner = new Mock<IPetRepository>();
        inner.Setup(x => x.FindAllAsync()).ReturnsAsync([]);
        var writer = new Mock<IAuditWriter>(MockBehavior.Strict);
        await new AuditedPetRepository(inner.Object, writer.Object).FindAllAsync();
        writer.VerifyNoOtherCalls();
    }
}
