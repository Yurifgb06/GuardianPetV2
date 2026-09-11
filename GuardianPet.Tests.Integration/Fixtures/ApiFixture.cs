using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using GuardianPet.Data;
using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace GuardianPet.Tests.Integration.Fixtures;

public sealed class ApiFixture : IAsyncLifetime
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly string _schema = "GPTEST_" + Guid.NewGuid().ToString("N")[..20].ToUpperInvariant();
    private readonly string _password = "P" + Guid.NewGuid().ToString("N");
    private bool _schemaCreated;
    public CustomWebApplicationFactory Factory { get; private set; } = null!;
    public string AdminEmail { get; } = "admin-" + Guid.NewGuid().ToString("N") + "@example.test";
    public string AdminPassword { get; } = "Admin-" + Guid.NewGuid().ToString("N");
    private static long _cpf = 10000000000;
    public static UserRequestDTO NewUser() => new()
    {
        Name = "Integration User", Email = Guid.NewGuid().ToString("N") + "@example.test",
        Password = "Test-" + Guid.NewGuid().ToString("N"),
        Cpf = Interlocked.Increment(ref _cpf).ToString(), Phone = "11900000000"
    };
    public static PetRequestDTO NewPet() => new()
    {
        Name = "Integration Pet", Species = "Dog", Breed = "Labrador", Age = 3, Weight = 20
    };

    public async Task InitializeAsync()
    {
        try
        {
            await SqlAsync($"CREATE USER {_schema} IDENTIFIED BY \"{_password}\" QUOTA 100M ON USERS;");
            _schemaCreated = true;
            await SqlAsync($"GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE, CREATE TRIGGER TO {_schema};");
            var source = Environment.GetEnvironmentVariable("GUARDIANPET_TEST_ORACLE_SOURCE") ?? "localhost:1521/FREEPDB1";
            Factory = new CustomWebApplicationFactory($"User Id={_schema};Password={_password};Data Source={source};Pooling=false",
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Empty(await db.Users.ToListAsync());
            var admin = new User { Name = "Integration Admin", Email = AdminEmail, Cpf = "99999999999", Phone = "11900000000" };
            admin.Password = new PasswordHasher<User>().HashPassword(admin, AdminPassword);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
            Assert.Equal(1, admin.Id);
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public HttpClient Client() => Factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    public async Task<HttpClient> AuthenticatedClientAsync(bool admin = false)
    {
        var client = Client();
        var email = AdminEmail;
        var password = AdminPassword;
        if (!admin)
        {
            var dto = NewUser();
            using var created = await client.PostAsJsonAsync("/api/users", dto);
            created.EnsureSuccessStatusCode();
            email = dto.Email;
            password = dto.Password;
        }
        using var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<LoginResponseDTO>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (Factory is not null) await Factory.DisposeAsync();
        }
        finally
        {
            if (_schemaCreated)
            {
                if (!Regex.IsMatch(_schema, "^GPTEST_[A-F0-9]{20}$")) throw new InvalidOperationException("Unsafe schema name.");
                await SqlAsync($"DROP USER {_schema} CASCADE;");
                _schemaCreated = false;
            }
        }
    }

    private static async Task SqlAsync(string sql)
    {
        var start = new ProcessStartInfo("docker")
        {
            RedirectStandardInput = true, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true
        };
        foreach (var arg in new[] { "exec", "-i", Environment.GetEnvironmentVariable("GUARDIANPET_TEST_ORACLE_CONTAINER") ?? "oracle-db",
            "sqlplus", "-s", "/", "as", "sysdba" }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Docker could not start.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.StandardInput.WriteLineAsync("WHENEVER SQLERROR EXIT SQL.SQLCODE");
        await process.StandardInput.WriteLineAsync("ALTER SESSION SET CONTAINER=FREEPDB1;");
        await process.StandardInput.WriteLineAsync(sql);
        await process.StandardInput.WriteLineAsync("EXIT");
        process.StandardInput.Close();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(true); throw new TimeoutException("Oracle test setup timed out."); }
        var stdout = await output;
        var stderr = await error;
        if (process.ExitCode != 0 || stdout.Contains("ORA-") || stderr.Contains("Error"))
            throw new InvalidOperationException($"Oracle fixture failed ({Regex.Match(stdout, @"ORA-\d+").Value}). Start oracle-db and verify Docker access; no application schema is used.");
    }
}
