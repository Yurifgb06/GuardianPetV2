using System.Net;
using System.Net.Http.Json;
using GuardianPet.Audit;
using GuardianPet.Data;
using GuardianPet.DTOs.Response;
using GuardianPet.Models;
using GuardianPet.Repositories;
using GuardianPet.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace GuardianPet.Tests.Integration.Endpoints;

[Collection("Oracle integration")]
public sealed class AuditEndpointsTests(ApiFixture fixture)
{
    [MongoFact]
    public async Task HttpCrud_PersistsMinimalAuditWithSameCorrelationId()
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var correlation = "audit-" + Guid.NewGuid().ToString("N");
        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlation);
        var dto = ApiFixture.NewPet();
        using var created = await client.PostAsJsonAsync("/api/pets", dto);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var pet = (await created.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions))!;
        dto.Name = "Changed";
        using var updated = await client.PutAsJsonAsync("/api/pets/" + pet.Id, dto);
        using var deleted = await client.DeleteAsync("/api/pets/" + pet.Id);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var response = await client.GetAsync($"/api/audit-events?entityType=Pet&entityId={pet.Id}&correlationId={correlation}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = (await response.Content.ReadFromJsonAsync<List<AuditEvent>>())!;
        Assert.Equal(new[] { "Created", "Deleted", "Updated" }, events.Select(x => x.Action).OrderBy(x => x));
        Assert.All(events, x => { Assert.Equal(correlation, x.CorrelationId); Assert.Equal(DateTimeKind.Utc, x.OccurredAtUtc.Kind); });
        var db = fixture.Factory.Services.GetRequiredService<IMongoClient>().GetDatabase(fixture.Factory.MongoDatabaseName);
        var documents = await db.GetCollection<BsonDocument>("audit_events").Find(new BsonDocument("entityId", pet.Id)).ToListAsync();
        Assert.Equal(3, documents.Count(x => x["entityType"] == "Pet"));
        foreach (var document in documents.Where(x => x["entityType"] == "Pet"))
            Assert.Equal(new[] { "_id", "action", "correlationId", "entityId", "entityType", "occurredAtUtc" }, document.Names.OrderBy(x => x));
    }

    [MongoTheory]
    [InlineData("User")]
    [InlineData("Veterinarian")]
    [InlineData("Consultation")]
    public async Task Repositories_PersistAllBusinessActionsInRealMongo(string entityType)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var key = Guid.NewGuid().ToString("N");
        var user = new User { Name = "Audit user", Email = key + "@test.example", Password = "unused", Cpf = key, Phone = "11900000000" };
        var pet = new Pet { Name = "Audit pet", Species = "Dog", Breed = "Test", Weight = 1 };
        var vet = new Veterinarian { Name = "Audit vet", Email = key + "@vet.example", Crmv = key, Phone = "11900000000", YearsExperience = 1 };
        long id;
        if (entityType == "User")
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            id = (await repository.CreateAsync(user)).Id;
            user.Name = "Updated";
            await repository.UpdateAsync(user);
            await repository.DeleteAsync(user);
        }
        else if (entityType == "Veterinarian")
        {
            var repository = scope.ServiceProvider.GetRequiredService<IVeterinarianRepository>();
            id = (await repository.CreateAsync(vet)).Id;
            vet.Name = "Updated";
            await repository.UpdateAsync(vet);
            await repository.DeleteAsync(vet);
        }
        else
        {
            db.Users.Add(user); db.Pets.Add(pet); db.Veterinarians.Add(vet);
            await db.SaveChangesAsync();
            var consultation = new Consultation { UserId = user.Id, PetId = pet.Id, VeterinarianId = vet.Id,
                ConsultationDate = DateTime.UtcNow, Symptoms = "Sensitive symptoms", Diagnosis = "Sensitive diagnosis",
                Treatment = "Sensitive treatment", Observations = "Sensitive observations" };
            var repository = scope.ServiceProvider.GetRequiredService<IConsultationRepository>();
            id = (await repository.CreateAsync(consultation)).Id;
            consultation.Diagnosis = "Changed sensitive diagnosis";
            await repository.UpdateAsync(consultation);
            await repository.DeleteAsync(consultation);
        }
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var events = await client.GetFromJsonAsync<List<AuditEvent>>($"/api/audit-events?entityType={entityType}&entityId={id}");
        Assert.Equal(new[] { "Created", "Deleted", "Updated" }, events!.Select(x => x.Action).OrderBy(x => x));
    }

    [MongoFact]
    public async Task MongoFiltersAndPagination_RunAgainstIsolatedDatabase()
    {
        var correlation = "query-" + Guid.NewGuid().ToString("N");
        using var scope = fixture.Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (var i = 0; i < 5; i++)
            await repository.InsertAsync(new AuditEvent { EntityType = "Pet", EntityId = 987654, Action = "Updated",
                CorrelationId = correlation, OccurredAtUtc = date.AddDays(i) }, default);
        await repository.InsertAsync(new AuditEvent { EntityType = "Pet", EntityId = 987654, Action = "Deleted",
            CorrelationId = correlation, OccurredAtUtc = date }, default);
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var query = $"/api/audit-events?entityType=Pet&entityId=987654&action=Updated&correlationId={correlation}&fromUtc=2026-01-01T00:00:00Z&toUtc=2026-01-05T00:00:00Z&pageSize=2";
        var first = (await client.GetFromJsonAsync<List<AuditEvent>>(query + "&page=1"))!;
        var middle = (await client.GetFromJsonAsync<List<AuditEvent>>(query + "&page=2"))!;
        var last = (await client.GetFromJsonAsync<List<AuditEvent>>(query + "&page=3"))!;
        var empty = (await client.GetFromJsonAsync<List<AuditEvent>>(query + "&page=4"))!;
        Assert.Equal(new[] { date.AddDays(4), date.AddDays(3) }, first.Select(x => x.OccurredAtUtc));
        Assert.Equal(new[] { date.AddDays(2), date.AddDays(1) }, middle.Select(x => x.OccurredAtUtc));
        Assert.Equal(date, Assert.Single(last).OccurredAtUtc);
        Assert.Empty(empty);
        using var linked = await client.GetAsync(query + "&page=2&includeLinks=true");
        Assert.Equal(HttpStatusCode.OK, linked.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await linked.Content.ReadAsStringAsync());
        Assert.Equal(new[] { "self", "first", "prev", "next", "last" }, json.RootElement.GetProperty("links").EnumerateArray()
            .Select(x => x.GetProperty("rel").GetString()));
        Assert.All(json.RootElement.GetProperty("items").EnumerateArray(), x => Assert.Empty(x.GetProperty("links").EnumerateArray()));
    }

    [MongoFact]
    public async Task InvalidCrud_DoesNotCreateAuditEvent()
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var correlation = "failed-" + Guid.NewGuid().ToString("N");
        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlation);
        var invalid = ApiFixture.NewPet(); invalid.Weight = -1;
        using var failed = await client.PostAsJsonAsync("/api/pets", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, failed.StatusCode);
        var events = await client.GetFromJsonAsync<List<AuditEvent>>("/api/audit-events?correlationId=" + correlation);
        Assert.Empty(events!);
    }

    [MongoFact]
    public async Task OracleFailure_DoesNotCreateAuditEvent()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User { Name = "Test", Email = fixture.AdminEmail, Password = "unused", Cpf = Guid.NewGuid().ToString("N"), Phone = "11900000000" };
        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => repository.CreateAsync(user));
        var audit = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
        Assert.Empty(await audit.QueryAsync(new AuditQuery { EntityType = "User", EntityId = user.Id }, default));
    }

    [MongoFact]
    public async Task MongoUnavailable_DoesNotRollbackOracleAndReadReturnsSafe503()
    {
        using var authenticated = await fixture.AuthenticatedClientAsync(admin: true);
        await using var factory = fixture.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMongoClient>();
            var settings = MongoClientSettings.FromConnectionString("mongodb://127.0.0.1:1");
            settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(100);
            settings.ConnectTimeout = TimeSpan.FromMilliseconds(100);
            services.AddSingleton<IMongoClient>(new MongoClient(settings));
        }));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        using var created = await client.PostAsJsonAsync("/api/pets", ApiFixture.NewPet());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var pet = (await created.Content.ReadFromJsonAsync<PetResponseDTO>(ApiFixture.JsonOptions))!;
        using var read = await authenticated.GetAsync("/api/pets/" + pet.Id);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        using var response = await client.GetAsync("/api/audit-events");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("Auditoria temporariamente", text);
        Assert.DoesNotContain("mongodb", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("127.0.0.1", text);
        Assert.Empty((await authenticated.GetFromJsonAsync<List<AuditEvent>>("/api/audit-events?entityType=Pet&entityId=" + pet.Id))!);
    }

    [Fact]
    public async Task AdministrativeEndpoint_RequiresAuthentication()
    {
        using var client = fixture.Client();
        using var response = await client.GetAsync("/api/audit-events");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdministrativeEndpoint_RequiresManageUsers()
    {
        using var client = await fixture.AuthenticatedClientAsync();
        using var response = await client.GetAsync("/api/audit-events");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("entityType=Unknown")]
    [InlineData("action=Unknown")]
    [InlineData("fromUtc=2026-02-01&toUtc=2026-01-01")]
    public async Task InvalidQuery_Returns400(string query)
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        using var response = await client.GetAsync("/api/audit-events?" + query);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
