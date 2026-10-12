using GuardianPet.Data;
using GuardianPet.DTOs.Query;
using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GuardianPet.Tests.Unit.Queries;

public sealed class CollectionQueryTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public void InvalidBounds_Return400(int page, int size)
    {
        var error = Assert.Throws<ApiException>(() => new PetQueryDTO { Page = page, PageSize = size }.EnsureValid());
        Assert.Equal(400, error.StatusCode);
    }

    [Theory]
    [InlineData("password", "asc")]
    [InlineData("name; DROP TABLE tb_pets", "asc")]
    [InlineData("name", "sideways")]
    [InlineData("", "asc")]
    [InlineData("name", "")]
    public void InvalidSort_Return400(string field, string direction)
    {
        Assert.Equal(400, Assert.Throws<ApiException>(() =>
            new PetQueryDTO { SortBy = field, SortDirection = direction }.EnsureValid()).StatusCode);
    }

    [Fact]
    public void InvalidDateInterval_Return400()
    {
        Assert.Equal(400, Assert.Throws<ApiException>(() => new ConsultationQueryDTO
        { DateFrom = new DateTime(2026, 2, 1), DateTo = new DateTime(2026, 1, 1) }.EnsureValid()).StatusCode);
    }

    [Theory]
    [InlineData(1, 2, "asc", 1, 2)]
    [InlineData(2, 2, "asc", 3, 0)]
    [InlineData(3, 2, "asc", 0, 0)]
    [InlineData(1, 2, "desc", 3, 1)]
    public void Pages_UseIdToBreakTies(int page, int size, string direction, long first, long second)
    {
        var data = new[] { new Pet { Id = 3, Name = "B" }, new Pet { Id = 2, Name = "A" }, new Pet { Id = 1, Name = "A" } }.AsQueryable();
        var options = new PetQueryDTO { Page = page, PageSize = size, SortBy = "name", SortDirection = direction };
        var result = CollectionQuery.Apply(data, options, x => x.Name, x => x.Id).Select(x => x.Id).ToArray();
        Assert.Equal(new[] { first, second }.Where(x => x != 0), result);
    }

    [Fact]
    public void WithoutOptions_PreservesOriginalQueryAndOrder()
    {
        var data = new[] { new Pet { Id = 3 }, new Pet { Id = 1 } }.AsQueryable();
        Assert.Same(data, CollectionQuery.Apply(data, new PetQueryDTO(), x => x.Id, x => x.Id));
    }

    [Theory]
    [InlineData(1, null, 20)]
    [InlineData(null, 2, 2)]
    [InlineData(1, 100, 100)]
    public void SinglePaginationParameter_UsesDocumentedDefaults(int? page, int? size, int expected)
    {
        var data = Enumerable.Range(1, 120).Reverse().Select(id => new Pet { Id = id }).AsQueryable();
        var result = CollectionQuery.Apply(data, new PetQueryDTO { Page = page, PageSize = size }, x => x.Id, x => x.Id).ToArray();
        Assert.Equal(expected, result.Length);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void OracleTranslation_ContainsOrderAndDatabasePagination()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseOracle("User Id=unused;Password=unused;Data Source=localhost:1521/FREEPDB1").Options);
        var query = db.Pets.Where(x => x.Species == "Dog");
        var sql = CollectionQuery.Apply(query, new PetQueryDTO { Page = 2, PageSize = 5, SortBy = "name" },
            x => x.Name, x => x.Id).ToQueryString();
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("OFFSET", sql);
        Assert.Contains("FETCH", sql);
        Assert.Contains("WHERE", sql);
    }
}
