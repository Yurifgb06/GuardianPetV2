using System.ComponentModel.DataAnnotations;

namespace GuardianPet.Audit;

public sealed class MongoAuditOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "guardianpet_audit";
    public string CollectionName { get; set; } = "audit_events";
    [Range(100, 10000)]
    public int TimeoutMilliseconds { get; set; } = 1000;
}
