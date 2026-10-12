using System.Net;
using System.Text.Json;
using GuardianPet.Data;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuardianPet.Tests.Integration.Endpoints;

[Collection("Oracle integration")]
public sealed class CollectionQueryEndpointsTests(ApiFixture fixture)
{
    public static IEnumerable<object[]> InvalidQueries()
    {
        string[] paths = ["/api/pets", "/api/pets/search", "/api/users", "/api/users/search?name=Test",
            "/api/veterinarians", "/api/veterinarians/search/name?name=Test",
            "/api/veterinarians/search/specialty?specialty=CLINICA_GERAL", "/api/consultations"];
        string[] queries = ["page=0", "page=-1", "pageSize=0", "pageSize=-1", "pageSize=101",
            "page=abc", "page=2147483647&pageSize=100", "sortBy=password",
            "sortBy=name%3BDROP", "sortDirection=sideways"];
        foreach (var path in paths)
        foreach (var query in queries) yield return [path, query];
    }

    [Theory]
    [MemberData(nameof(InvalidQueries))]
    public async Task InvalidParameters_Return400(string path, string query)
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        using var response = await client.GetAsync(path + (path.Contains('?') ? "&" : "?") + query);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("pets")]
    [InlineData("users")]
    [InlineData("veterinarians")]
    [InlineData("consultations")]
    public async Task OracleCollections_FilterSortAndPage_PreserveArrayContract(string entity)
    {
        var seeded = await SeedAsync();
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var filter = entity switch
        {
            "pets" => "species=" + seeded.Marker + "&petSize=SMALL",
            "users" => "name=" + seeded.Marker,
            "veterinarians" => "name=" + seeded.Marker + "&specialty=CLINICA_GERAL",
            _ => $"petId={seeded.PetId}&userId={seeded.UserId}&veterinarianId={seeded.VetId}&status=SCHEDULED&dateFrom=2026-01-01&dateTo=2026-01-31"
        };
        var sort = entity == "consultations" ? "consultationDate" : "name";
        var root = "/api/" + entity + "?" + filter;
        var all = await ReadArrayAsync(client, root);
        Assert.Equal(3, all.Length);
        var expected = entity switch
        {
            "pets" => seeded.Pets,
            "users" => seeded.Users,
            "veterinarians" => seeded.Vets,
            _ => seeded.Consultations
        };
        var first = await ReadArrayAsync(client, root + $"&sortBy={sort}&sortDirection=asc&page=1&pageSize=2");
        var last = await ReadArrayAsync(client, root + $"&sortBy={sort}&sortDirection=asc&page=2&pageSize=2");
        var empty = await ReadArrayAsync(client, root + $"&sortBy={sort}&page=3&pageSize=2");
        var descending = await ReadArrayAsync(client, root + $"&sortBy={sort}&sortDirection=desc&page=1&pageSize=2");
        // Seed order is B, A, A (or day 2, day 1, day 1); ID breaks equal values.
        Assert.Equal(new[] { expected[1], expected[2] }, Ids(first));
        Assert.Equal(new[] { expected[0] }, Ids(last));
        Assert.Empty(empty);
        Assert.Equal(new[] { expected[0], expected[1] }, Ids(descending));
        Assert.Equal(new[] { expected[1], expected[2], expected[0] },
            Ids(await ReadArrayAsync(client, root + $"&sortBy={sort}&sortDirection=asc")));
        Assert.Equal(new[] { expected[0], expected[1], expected[2] },
            Ids(await ReadArrayAsync(client, root + $"&sortBy={sort}&sortDirection=desc")));
        var defaultOrder = await ReadArrayAsync(client, root + "&pageSize=2");
        Assert.Equal(expected.OrderBy(x => x).Take(2), Ids(defaultOrder));
        Assert.Equal(3, (await ReadArrayAsync(client, root + "&page=1")).Length);
        var old = await ReadArrayAsync(client, "/api/" + entity);
        foreach (var id in expected) Assert.Contains(id, Ids(old));
        Assert.Equal(Fields(entity), all[0].EnumerateObject().Select(p => p.Name).OrderBy(x => x));

        if (entity == "users")
        {
            var email = all[0].GetProperty("email").GetString()!;
            var exact = await ReadArrayAsync(client, root + "&email=" + email.ToUpperInvariant() + "&pageSize=1");
            Assert.Equal(all[0].GetProperty("id").GetInt64(), Assert.Single(exact).GetProperty("id").GetInt64());
            using var response = await client.GetAsync("/api/users/email?email=" + email);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(JsonValueKind.Object, json.RootElement.ValueKind);
        }
        if (entity == "pets")
        {
            Assert.Equal(Ids(all).OrderBy(x => x), Ids(await ReadArrayAsync(client, "/api/pets/search?" + filter)).OrderBy(x => x));
            Assert.Equal(Ids(first), Ids(await ReadArrayAsync(client, "/api/pets/search?" + filter + $"&sortBy={sort}&pageSize=2")));
        }
        if (entity == "users")
            Assert.Equal(Ids(all).OrderBy(x => x), Ids(await ReadArrayAsync(client, "/api/users/search?name=" + seeded.Marker)).OrderBy(x => x));
        if (entity == "veterinarians")
        {
            Assert.Equal(Ids(all).OrderBy(x => x), Ids(await ReadArrayAsync(client, "/api/veterinarians/search/name?" + filter)).OrderBy(x => x));
            var specialty = await ReadArrayAsync(client, "/api/veterinarians/search/specialty?specialty=CLINICA_GERAL");
            foreach (var id in expected) Assert.Contains(id, Ids(specialty));
            Assert.Equal(Ids(first), Ids(await ReadArrayAsync(client, "/api/veterinarians/search/specialty?" + filter + "&sortBy=name&pageSize=2")));
        }
    }

    [Theory]
    [InlineData("/api/users/search")]
    [InlineData("/api/veterinarians/search/name")]
    public async Task LegacyRequiredName_RemainsRequired(string path)
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/pets?page=1&pageSize=1")]
    [InlineData("/api/users?page=1&pageSize=1")]
    [InlineData("/api/veterinarians?page=1&pageSize=1")]
    [InlineData("/api/consultations?page=1&pageSize=1")]
    public async Task NewParameters_StillRequireAuthentication(string path)
    {
        using var client = fixture.Client();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UserCollection_WithParameters_StillRequiresManageUsers()
    {
        using var client = await fixture.AuthenticatedClientAsync();
        using var response = await client.GetAsync("/api/users?page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/pets?petSize=999")]
    [InlineData("/api/veterinarians?specialty=999")]
    [InlineData("/api/consultations?status=999")]
    [InlineData("/api/consultations?dateFrom=invalid")]
    [InlineData("/api/consultations?dateFrom=2026-02-01&dateTo=2026-01-01")]
    public async Task InvalidFilters_Return400(string path)
    {
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_DescribesOptionalQueryParametersAndArrayResponses()
    {
        using var client = fixture.Client();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        foreach (var path in new[] { "/api/pets", "/api/pets/search", "/api/users", "/api/users/search",
            "/api/veterinarians", "/api/veterinarians/search/name", "/api/veterinarians/search/specialty", "/api/consultations" })
        {
            var get = document.RootElement.GetProperty("paths").GetProperty(path).GetProperty("get");
            var parameters = get.GetProperty("parameters").EnumerateArray().ToArray();
            foreach (var name in new[] { "page", "pageSize", "sortBy", "sortDirection" })
            {
                var parameter = Assert.Single(parameters, p => string.Equals(p.GetProperty("name").GetString(), name, StringComparison.OrdinalIgnoreCase));
                Assert.False(parameter.TryGetProperty("required", out var required) && required.GetBoolean());
            }
            Assert.True(get.GetProperty("responses").TryGetProperty("400", out _));
        }
    }

    [Theory]
    [InlineData("pets", "petSize", "asc")]
    [InlineData("pets", "petSize", "desc")]
    [InlineData("veterinarians", "yearsExperience", "asc")]
    [InlineData("veterinarians", "yearsExperience", "desc")]
    [InlineData("consultations", "consultationDate", "asc")]
    [InlineData("consultations", "consultationDate", "desc")]
    public async Task CamelCaseSortFields_OrderByRequestedField(string entity, string field, string direction)
    {
        var seeded = await SeedAsync();
        using var client = await fixture.AuthenticatedClientAsync(admin: true);
        var filter = entity == "pets" ? "species=" + seeded.Marker :
            entity == "veterinarians" ? "name=" + seeded.Marker : "petId=" + seeded.PetId;
        var all = await ReadArrayAsync(client, "/api/" + entity + "?" + filter);
        Func<JsonElement, double> value = entity == "pets"
            ? x => x.GetProperty(field).GetString() == "BIG" ? 2 : 0
            : entity == "veterinarians" ? x => x.GetProperty(field).GetInt32()
            : x => x.GetProperty(field).GetDateTime().Ticks;
        var ordered = direction == "asc" ? all.OrderBy(value) : all.OrderByDescending(value);
        var expected = ordered.ThenBy(x => x.GetProperty("id").GetInt64()).Take(2).Select(x => x.GetProperty("id").GetInt64());
        var actual = await ReadArrayAsync(client, "/api/" + entity + "?" + filter + $"&sortBy={field}&sortDirection={direction}&pageSize=2");
        Assert.Equal(expected, Ids(actual));
    }

    private static async Task<JsonElement[]> ReadArrayAsync(HttpClient client, string path)
    {
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        return document.RootElement.EnumerateArray().Select(x => x.Clone()).ToArray();
    }

    private static long[] Ids(JsonElement[] items) => items.Select(x => x.GetProperty("id").GetInt64()).ToArray();

    private static string[] Fields(string entity) => (entity switch
    {
        "pets" => new[] { "id", "name", "species", "breed", "ageType", "age", "weight", "petSize" },
        "users" => ["id", "name", "email", "phone"],
        "veterinarians" => ["id", "name", "email", "crmv", "specialty", "phone", "yearsExperience"],
        _ => ["id", "consultationDate", "symptoms", "diagnosis", "treatment", "observations", "status"]
    }).OrderBy(x => x).ToArray();

    private async Task<Seed> SeedAsync()
    {
        // All writes use the fixture's isolated GPTEST schema.
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var marker = "Q" + Guid.NewGuid().ToString("N");
        var users = new List<User>();
        var pets = new List<Pet>();
        var vets = new List<Veterinarian>();
        for (var i = 0; i < 4; i++)
        {
            var suffix = i == 0 ? "B" : "A";
            users.Add(new User { Name = marker + suffix, Email = marker + i + "@example.test",
                Password = "unused-query-test", Cpf = marker + i, Phone = "11900000000" });
            pets.Add(new Pet { Name = marker + suffix, Species = marker, Breed = "Test", Age = 2, Weight = 10,
                AgeType = AgeTypeEnum.ANOS, PetSize = i == 3 ? PetSizeEnum.BIG : PetSizeEnum.SMALL });
            vets.Add(new Veterinarian { Name = marker + suffix, Email = marker + i + "@vet.test", Crmv = marker + i,
                Phone = "11900000000", YearsExperience = i == 0 ? 3 : 1, Specialty = i == 3 ? SpecialtyEnum.CIRURGIA : SpecialtyEnum.CLINICA_GERAL });
        }
        // Fourth user must not match the name filter.
        users[3].Name = "Other";
        db.Users.AddRange(users);
        db.Pets.AddRange(pets);
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();
        var consultations = Enumerable.Range(0, 5).Select(i => new Consultation
        {
            UserId = users[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id,
            ConsultationDate = new DateTime(2026, i == 4 ? 2 : 1, i == 0 ? 2 : 1),
            Symptoms = "Symptoms", Diagnosis = "Diagnosis", Treatment = "Treatment", Observations = "Observation",
            Status = i == 3 ? StatusConsultationEnum.COMPLETED : StatusConsultationEnum.SCHEDULED
        }).ToArray();
        db.Consultations.AddRange(consultations);
        await db.SaveChangesAsync();
        return new Seed(marker, users.Take(3).Select(x => x.Id).ToArray(), pets.Take(3).Select(x => x.Id).ToArray(),
            vets.Take(3).Select(x => x.Id).ToArray(), consultations.Take(3).Select(x => x.Id).ToArray(), users[0].Id, pets[0].Id, vets[0].Id);
    }

    private sealed record Seed(string Marker, long[] Users, long[] Pets, long[] Vets, long[] Consultations, long UserId, long PetId, long VetId);
}
