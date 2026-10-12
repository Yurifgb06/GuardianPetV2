using System.Text.Json;
using GuardianPet.Audit;
using GuardianPet.Controllers;
using GuardianPet.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace GuardianPet.Tests.Unit.Audit;

public sealed class AuditServiceTests
{
    private readonly Mock<IAuditRepository> _repository = new();
    private readonly Mock<ILogger<AuditService>> _logger = new();
    private AuditService Service()
    {
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = "audit-test-correlation";
        var services = new ServiceCollection().AddSingleton(_repository.Object).BuildServiceProvider();
        return new AuditService(services, new HttpContextAccessor { HttpContext = context },
            Options.Create(new MongoAuditOptions { TimeoutMilliseconds = 100 }), _logger.Object);
    }

    [Fact]
    public async Task Record_ContainsOnlyMinimalMetadataAndUtc()
    {
        AuditEvent? saved = null;
        _repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEvent, CancellationToken>((value, _) => saved = value).Returns(Task.CompletedTask);
        var before = DateTime.UtcNow;
        await Service().RecordAsync("Pet", 7, "Created");
        Assert.NotNull(saved);
        Assert.Equal(DateTimeKind.Utc, saved.OccurredAtUtc.Kind);
        Assert.InRange(saved.OccurredAtUtc, before, DateTime.UtcNow);
        Assert.Equal("audit-test-correlation", saved.CorrelationId);
        Assert.True(ObjectId.TryParse(saved.Id, out _));
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(saved, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Equal(new[] { "action", "correlationId", "entityId", "entityType", "id", "occurredAtUtc" },
            json.RootElement.EnumerateObject().Select(x => x.Name).OrderBy(x => x));
    }

    [Fact]
    public async Task WriteTimeout_IsBoundedAndSafeLogDoesNotExposeException()
    {
        _repository.Setup(x => x.InsertAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .Returns<AuditEvent, CancellationToken>((_, token) => Task.Delay(Timeout.Infinite, token));
        await Service().RecordAsync("Pet", 7, "Created");
        var log = Assert.Single(_logger.Invocations);
        Assert.Equal(LogLevel.Warning, log.Arguments[0]);
        Assert.Null(log.Arguments[3]);
        Assert.Equal("Falha na gravação de auditoria; operação Oracle preservada.", log.Arguments[2].ToString());
    }

    [Fact]
    public async Task ReadFailure_ReturnsSafe503()
    {
        _repository.Setup(x => x.QueryAsync(It.IsAny<AuditQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("mongodb://credential@example/internal"));
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().QueryAsync(new AuditQuery(), default));
        Assert.Equal(503, error.StatusCode);
        Assert.Equal("Auditoria temporariamente indisponível.", error.Message);
        Assert.Null(Assert.Single(_logger.Invocations).Arguments[3]);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task InvalidBounds_DoNotQueryMongo(int page, int size)
    {
        var error = await Assert.ThrowsAsync<ApiException>(() => Service().QueryAsync(new AuditQuery { Page = page, PageSize = size }, default));
        Assert.Equal(400, error.StatusCode);
        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Password", "Created")]
    [InlineData("Pet", "Read")]
    public void InvalidFilters_AreRejected(string entityType, string action)
        => Assert.Throws<ApiException>(() => new AuditQuery { EntityType = entityType, Action = action }.EnsureValid());

    [Fact]
    public void CombinedFilter_IsTypedAndContainsAllConstraints()
    {
        var query = new AuditQuery { EntityType = "Pet", EntityId = 42, Action = "Updated",
            CorrelationId = "correlation", FromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) };
        var filter = MongoAuditRepository.BuildFilter(query).Render(new RenderArgs<AuditEvent>(
            BsonSerializer.SerializerRegistry.GetSerializer<AuditEvent>(), BsonSerializer.SerializerRegistry));
        Assert.Equal("Pet", filter["entityType"].AsString);
        Assert.Equal(42, filter["entityId"].AsInt64);
        Assert.Equal("Updated", filter["action"].AsString);
        Assert.Equal("correlation", filter["correlationId"].AsString);
        Assert.Equal(query.FromUtc, filter["occurredAtUtc"]["$gte"].ToUniversalTime());
        Assert.Equal(query.ToUtc, filter["occurredAtUtc"]["$lte"].ToUniversalTime());
    }

    [Fact]
    public void AdministrativeEndpoint_RequiresExistingPolicy()
        => Assert.Equal("ManageUsers", Assert.Single(typeof(AuditController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()).Policy);

    [Fact]
    public async Task Read_PreservesFiltersAndPageBeforeRepository()
    {
        var query = new AuditQuery { EntityType = "User", Page = 2, PageSize = 1 };
        _repository.Setup(x => x.QueryAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync([new AuditEvent { EntityId = 42 }]);
        Assert.Equal(42, Assert.Single(await Service().QueryAsync(query, default)).EntityId);
        _repository.Verify(x => x.CountAsync(It.IsAny<AuditQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
