using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
namespace GuardianPet.Tests.Integration.Fixtures;
public sealed class CustomWebApplicationFactory(string connectionString, string jwtKey) : WebApplicationFactory<Program>
{
    public string MongoDatabaseName { get; } = "GPTEST_" + Guid.NewGuid().ToString("N");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("MongoDB:ConnectionString",
            Environment.GetEnvironmentVariable("GUARDIANPET_TEST_MONGO_CONNECTION") ?? "mongodb://localhost:27017");
        builder.UseSetting("MongoDB:DatabaseName", MongoDatabaseName);
        builder.UseSetting("MongoDB:TimeoutMilliseconds", "1000");
        builder.UseSetting("ConnectionStrings:OracleConnection", connectionString);
        builder.UseSetting("Jwt:Issuer", "GuardianPet.Tests");
        builder.UseSetting("Jwt:Audience", "GuardianPet.Tests");
        builder.UseSetting("Jwt:Key", jwtKey);
        // Fresh schema; fixture inserts the administrator before any other user.
        builder.UseSetting("Jwt:AdminUserIds:0", "1");
    }
}
