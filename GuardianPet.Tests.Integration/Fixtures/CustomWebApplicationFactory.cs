using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
namespace GuardianPet.Tests.Integration.Fixtures;
public sealed class CustomWebApplicationFactory(string connectionString, string jwtKey) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:OracleConnection", connectionString);
        builder.UseSetting("Jwt:Issuer", "GuardianPet.Tests");
        builder.UseSetting("Jwt:Audience", "GuardianPet.Tests");
        builder.UseSetting("Jwt:Key", jwtKey);
        // Fresh schema; fixture inserts the administrator before any other user.
        builder.UseSetting("Jwt:AdminUserIds:0", "1");
    }
}
