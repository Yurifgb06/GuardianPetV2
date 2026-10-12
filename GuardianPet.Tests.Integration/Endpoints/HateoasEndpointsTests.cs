using System.Net;
using System.Text.Json;
using GuardianPet.Data;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuardianPet.Tests.Integration.Endpoints;

[Collection("Oracle integration")]
public sealed class HateoasEndpointsTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("pets")]
    [InlineData("users")]
    [InlineData("veterinarians")]
    [InlineData("consultations")]
    [InlineData("users/email")]
    public async Task Individual_OptInWrapsUnchangedDataAndLinksResolve(string entity)
    {
        var seed = await SeedAsync();
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var path = entity == "users/email" ? "/api/users/email?email=" + seed.Email :
            "/api/" + entity + "/" + seed.Id(entity);
        var original = await GetAsync(client, path);
        var disabled = await GetAsync(client, Append(path, "includeLinks=false"));
        var linked = await GetAsync(client, Append(path, "includeLinks=true"));
        Assert.Equal(original.GetRawText(), disabled.GetRawText());
        Assert.False(original.TryGetProperty("links", out _));
        Assert.Equal(original.GetRawText(), linked.GetProperty("data").GetRawText());
        Assert.Equal(new[] { "self", "collection" }, Relations(linked));
        await FollowLinksAsync(client, linked);
    }

    [Theory]
    [InlineData("pets", "")]
    [InlineData("pets", "/search")]
    [InlineData("users", "")]
    [InlineData("users", "/search")]
    [InlineData("veterinarians", "")]
    [InlineData("veterinarians", "/search/name")]
    [InlineData("veterinarians", "/search/specialty")]
    [InlineData("consultations", "")]
    public async Task Collections_FirstMiddleLastAndEmptyPages_PreserveFilters(string entity, string suffix)
    {
        var seed = await SeedAsync();
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var path = "/api/" + entity + suffix + "?" + seed.Filter(entity)
            + "&sortBy=id&sortDirection=desc&page=1&pageSize=2";
        var legacy = await GetAsync(client, path);
        var disabled = await GetAsync(client, path + "&includeLinks=false");
        Assert.Equal(JsonValueKind.Array, legacy.ValueKind);
        Assert.Equal(legacy.GetRawText(), disabled.GetRawText());
        var first = await GetAsync(client, path + "&includeLinks=true");
        Assert.Equal(new[] { "self", "first", "next", "last" }, Relations(first));
        Assert.Equal(legacy.EnumerateArray().Select(x => x.GetRawText()),
            first.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("data").GetRawText()));
        var middle = await GetAsync(client, Href(first, "next"));
        Assert.Equal(new[] { "self", "first", "prev", "next", "last" }, Relations(middle));
        var last = await GetAsync(client, Href(first, "last"));
        Assert.Equal(new[] { "self", "first", "prev", "last" }, Relations(last));
        Assert.Equal(1, last.GetProperty("items").GetArrayLength());
        Assert.Equal("3", QueryHelpers.ParseQuery(new Uri(Href(first, "last")).Query)["page"].ToString());
        var empty = await GetAsync(client, path.Replace("page=1&", "page=4&") + "&includeLinks=true");
        Assert.Empty(empty.GetProperty("items").EnumerateArray());
        Assert.DoesNotContain("next", Relations(empty));
        foreach (var response in new[] { first, middle, last, empty })
        foreach (var link in response.GetProperty("links").EnumerateArray())
        {
            var uri = new Uri(link.GetProperty("href").GetString()!);
            Assert.Equal("https", uri.Scheme);
            Assert.Equal("localhost", uri.Host);
            Assert.Equal("/api/" + entity + suffix, uri.AbsolutePath);
            var query = QueryHelpers.ParseQuery(uri.Query);
            var originalQuery = QueryHelpers.ParseQuery(new Uri("https://localhost" + path).Query);
            foreach (var parameter in originalQuery.Where(x => x.Key != "page"))
                Assert.Equal(parameter.Value.ToString(), query[parameter.Key].ToString());
            Assert.Equal("true", query["includeLinks"].ToString());
            Assert.Equal("GET", link.GetProperty("method").GetString());
        }
        await FollowLinksAsync(client, first);
        foreach (var item in first.GetProperty("items").EnumerateArray()) await FollowLinksAsync(client, item);
    }

    [Theory]
    [InlineData("pets")]
    [InlineData("users")]
    [InlineData("veterinarians")]
    [InlineData("consultations")]
    public async Task EmptyCollection_ReturnsItemsAndNoNextOrPrevious(string entity)
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var filter = entity == "pets" ? "species=Missing" + Guid.NewGuid().ToString("N") :
            entity == "consultations" ? "petId=-1" : "name=Missing" + Guid.NewGuid().ToString("N");
        var response = await GetAsync(client, "/api/" + entity + "?" + filter + "&pageSize=2&includeLinks=true");
        Assert.Empty(response.GetProperty("items").EnumerateArray());
        Assert.Equal(new[] { "self", "first", "last" }, Relations(response));
        Assert.Equal("1", QueryHelpers.ParseQuery(new Uri(Href(response, "last")).Query)["page"].ToString());
        var unpaged = await GetAsync(client, "/api/" + entity + "?" + filter + "&includeLinks=true");
        Assert.Empty(unpaged.GetProperty("items").EnumerateArray());
        Assert.Equal(new[] { "self" }, Relations(unpaged));
    }

    [Fact]
    public async Task PathBase_HostAndSchemeArePreserved_AndLinksResolve()
    {
        var seed = await SeedAsync();
        using var authenticated = await fixture.AuthenticatedClientAsync();
        await using var factory = fixture.Factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, PrefixStartupFilter>()));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://guardian.example/guardian/"), AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        var response = await GetAsync(client, "api/pets?" + seed.Filter("pets") + "&pageSize=2&includeLinks=true");
        foreach (var link in response.GetProperty("links").EnumerateArray()
            .Concat(response.GetProperty("items")[0].GetProperty("links").EnumerateArray()))
        {
            var uri = new Uri(link.GetProperty("href").GetString()!);
            Assert.Equal("https", uri.Scheme);
            Assert.Equal("guardian.example", uri.Host);
            Assert.StartsWith("/guardian/api/pets", uri.AbsolutePath);
        }
        await FollowLinksAsync(client, response);
        var individual = await GetAsync(client, response.GetProperty("items")[0].GetProperty("links")[0].GetProperty("href").GetString()!);
        await FollowLinksAsync(client, individual);
    }

    [Theory]
    [InlineData("/api/pets?includeLinks=true")]
    [InlineData("/api/pets/1?includeLinks=true")]
    [InlineData("/api/users?includeLinks=true")]
    [InlineData("/api/users/1?includeLinks=true")]
    [InlineData("/api/users/email?email=test@example.test&includeLinks=true")]
    [InlineData("/api/veterinarians?includeLinks=true")]
    [InlineData("/api/veterinarians/1?includeLinks=true")]
    [InlineData("/api/consultations?includeLinks=true")]
    [InlineData("/api/consultations/1?includeLinks=true")]
    public async Task LinkedEndpoints_RequireAuthentication(string path)
    {
        using var client = fixture.Client();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users?includeLinks=true")]
    [InlineData("/api/users/1?includeLinks=true")]
    [InlineData("/api/users/email?email=test@example.test&includeLinks=true")]
    public async Task LinkedUserEndpoints_RequirePermission(string path)
    {
        using var client = await fixture.AuthenticatedClientAsync();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrdinaryUser_ReceivesOnlyAuthorizedReadLinks()
    {
        var seed = await SeedAsync();
        using var client = await fixture.AuthenticatedClientAsync();
        var resource = await GetAsync(client, "/api/pets/" + seed.PetId + "?includeLinks=true");
        var collection = await GetAsync(client, "/api/pets?" + seed.Filter("pets") + "&pageSize=2&includeLinks=true");
        foreach (var response in new[] { resource, collection })
        foreach (var link in response.GetProperty("links").EnumerateArray())
        {
            Assert.Equal("GET", link.GetProperty("method").GetString());
            Assert.DoesNotContain("/api/users", link.GetProperty("href").GetString()!);
        }
        await FollowLinksAsync(client, resource);
        await FollowLinksAsync(client, collection);
    }

    [Theory]
    [InlineData("/api/pets/-1?includeLinks=true", 404)]
    [InlineData("/api/users/-1?includeLinks=true", 404)]
    [InlineData("/api/veterinarians/-1?includeLinks=true", 404)]
    [InlineData("/api/consultations/-1?includeLinks=true", 404)]
    [InlineData("/api/pets?includeLinks=invalid", 400)]
    [InlineData("/api/pets?includeLinks=true&page=0", 400)]
    public async Task Errors_KeepExistingStatusAndDoNotAddLinks(string path, int status)
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        using var response = await client.GetAsync(path);
        Assert.Equal(status, (int)response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(document.RootElement.TryGetProperty("links", out _));
    }

    [Fact]
    public async Task Swagger_DescribesOptionalLinksAndBothRepresentations()
    {
        using var client = fixture.Client();
        var document = await GetAsync(client, "/swagger/v1/swagger.json");
        foreach (var path in document.GetProperty("paths").EnumerateObject().Where(p => p.Name.StartsWith("/api/")))
        {
            if (!path.Value.TryGetProperty("get", out var operation)) continue;
            var parameter = Assert.Single(operation.GetProperty("parameters").EnumerateArray(),
                p => p.GetProperty("name").GetString() == "includeLinks");
            Assert.False(parameter.TryGetProperty("required", out var required) && required.GetBoolean());
            Assert.Equal("boolean", parameter.GetProperty("schema").GetProperty("type").GetString());
            var schema = operation.GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema");
            Assert.Equal(2, schema.GetProperty("oneOf").GetArrayLength());
            Assert.Contains("includeLinks=true", operation.GetProperty("description").GetString());
        }
    }

    private static async Task<JsonElement> GetAsync(HttpClient client, string path)
    {
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    private static string Append(string path, string query) => path + (path.Contains('?') ? "&" : "?") + query;
    private static string[] Relations(JsonElement json) => json.GetProperty("links").EnumerateArray().Select(x => x.GetProperty("rel").GetString()!).ToArray();
    private static string Href(JsonElement json, string rel) => json.GetProperty("links").EnumerateArray().Single(x => x.GetProperty("rel").GetString() == rel).GetProperty("href").GetString()!;

    private static async Task FollowLinksAsync(HttpClient client, JsonElement response)
    {
        foreach (var link in response.GetProperty("links").EnumerateArray())
        {
            Assert.Equal("GET", link.GetProperty("method").GetString());
            var href = link.GetProperty("href").GetString()!;
            using var linked = await client.GetAsync(href);
            Assert.Equal(HttpStatusCode.OK, linked.StatusCode);
        }
    }

    private async Task<Seed> SeedAsync()
    {
        // Data is written only to the fixture's isolated GPTEST schema.
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var marker = "H" + Guid.NewGuid().ToString("N");
        var users = Enumerable.Range(0, 5).Select(i => new User { Name = marker + i, Email = marker + i + "@example.test",
            Password = "unused-query-test", Cpf = marker + i, Phone = "11900000000" }).ToArray();
        var species = marker + " cão & dog";
        var pets = Enumerable.Range(0, 6).Select(i => new Pet { Name = marker + i, Species = species,
            Breed = "Test", Age = 2, Weight = 10, AgeType = AgeTypeEnum.ANOS, PetSize = i == 5 ? PetSizeEnum.BIG : PetSizeEnum.SMALL }).ToArray();
        var vets = Enumerable.Range(0, 6).Select(i => new Veterinarian { Name = marker + i, Email = marker + i + "@vet.test", Crmv = marker + i,
            Phone = "11900000000", YearsExperience = 2, Specialty = i == 5 ? SpecialtyEnum.CIRURGIA : SpecialtyEnum.CLINICA_GERAL }).ToArray();
        db.Users.AddRange(users);
        db.Pets.AddRange(pets);
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();
        var consultations = Enumerable.Range(0, 6).Select(i => new Consultation { UserId = users[0].Id, PetId = pets[0].Id,
            VeterinarianId = vets[0].Id, ConsultationDate = new DateTime(2026, 1, 1), Symptoms = "Symptoms", Diagnosis = "Diagnosis",
            Treatment = "Treatment", Observations = "Observation", Status = i == 5 ? StatusConsultationEnum.COMPLETED : StatusConsultationEnum.SCHEDULED }).ToArray();
        db.Consultations.AddRange(consultations);
        await db.SaveChangesAsync();
        return new Seed(marker, species, users[0].Email, users[0].Id, pets[0].Id, vets[0].Id, consultations[0].Id);
    }

    private sealed record Seed(string Marker, string Species, string Email, long UserId, long PetId, long VetId, long ConsultationId)
    {
        public long Id(string entity) => entity switch { "pets" => PetId, "users" => UserId, "veterinarians" => VetId, _ => ConsultationId };
        public string Filter(string entity) => entity switch
        {
            "pets" => "species=" + Uri.EscapeDataString(Species) + "&petSize=SMALL",
            "users" => "name=" + Marker,
            "veterinarians" => "name=" + Marker + "&specialty=CLINICA_GERAL",
            _ => $"userId={UserId}&petId={PetId}&veterinarianId={VetId}&status=SCHEDULED&dateFrom=2026-01-01&dateTo=2026-01-31"
        };
    }

    private sealed class PrefixStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.UsePathBase("/guardian");
            next(app);
        };
    }
}
