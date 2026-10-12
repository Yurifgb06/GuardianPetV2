namespace GuardianPet.Audit;

public interface IAuditWriter
{
    Task RecordAsync(string entityType, long entityId, string action);
}
