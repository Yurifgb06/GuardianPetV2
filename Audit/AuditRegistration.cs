using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GuardianPet.Audit;

public static class AuditRegistration
{
    public static IServiceCollection AddMongoAudit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddOptions<MongoAuditOptions>().Bind(configuration.GetSection("MongoDB")).ValidateDataAnnotations();
        services.AddSingleton<IMongoClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MongoAuditOptions>>().Value;
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
            settings.ConnectTimeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
            settings.SocketTimeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
            return new MongoClient(settings);
        });
        services.AddScoped<IAuditRepository, MongoAuditRepository>();
        services.AddScoped<AuditService>();
        services.AddScoped<IAuditWriter>(provider => provider.GetRequiredService<AuditService>());
        return services;
    }
}
