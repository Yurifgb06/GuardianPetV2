using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace GuardianPet.Tests.Integration.Fixtures;

public sealed class MongoFactAttribute : FactAttribute
{
    public MongoFactAttribute() => Skip = MongoTestAvailability.SkipReason;
}

public sealed class MongoTheoryAttribute : TheoryAttribute
{
    public MongoTheoryAttribute() => Skip = MongoTestAvailability.SkipReason;
}

internal static class MongoTestAvailability
{
    private static readonly Lazy<string?> Result = new(() =>
    {
        try
        {
            var settings = MongoClientSettings.FromConnectionString(
                Environment.GetEnvironmentVariable("GUARDIANPET_TEST_MONGO_CONNECTION") ?? "mongodb://localhost:27017");
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(1);
            settings.ConnectTimeout = TimeSpan.FromSeconds(1);
            settings.SocketTimeout = TimeSpan.FromSeconds(1);
            new MongoClient(settings).GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            return null;
        }
        catch { return "MongoDB local de testes indisponível; iniciar o perfil local-audit."; }
    });
    public static string? SkipReason => Result.Value;
}
