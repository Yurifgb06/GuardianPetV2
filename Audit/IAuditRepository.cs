namespace GuardianPet.Audit;

public interface IAuditRepository
{
    Task InsertAsync(AuditEvent auditEvent, CancellationToken cancellationToken);
    Task<List<AuditEvent>> QueryAsync(AuditQuery query, CancellationToken cancellationToken);
    Task<long> CountAsync(AuditQuery query, CancellationToken cancellationToken);
}
