using GuardianPet.Exceptions;
using Microsoft.Extensions.Options;

namespace GuardianPet.Audit;

// Best effort: Oracle has already committed before RecordAsync is called.
public sealed class AuditService(IServiceProvider services, IHttpContextAccessor context,
    IOptions<MongoAuditOptions> options, ILogger<AuditService> logger) : IAuditWriter
{
    public async Task RecordAsync(string entityType, long entityId, string action)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(options.Value.TimeoutMilliseconds));
            var auditEvent = new AuditEvent
            {
                EntityType = entityType, EntityId = entityId, Action = action, OccurredAtUtc = DateTime.UtcNow,
                CorrelationId = context.HttpContext?.Items["CorrelationId"] as string ?? Guid.NewGuid().ToString("D")
            };
            await services.GetRequiredService<IAuditRepository>().InsertAsync(auditEvent, timeout.Token);
        }
        catch (Exception)
        {
            // Do not log the exception, payload, credentials or connection string.
            logger.LogWarning("Falha na gravação de auditoria; operação Oracle preservada.");
        }
    }

    public async Task<List<AuditEvent>> QueryAsync(AuditQuery query, CancellationToken cancellationToken)
        => await ReadAsync(query, (repository, token) => repository.QueryAsync(query, token), cancellationToken);

    public async Task<long> CountAsync(AuditQuery query, CancellationToken cancellationToken)
        => await ReadAsync(query, (repository, token) => repository.CountAsync(query, token), cancellationToken);

    private async Task<T> ReadAsync<T>(AuditQuery query, Func<IAuditRepository, CancellationToken, Task<T>> read,
        CancellationToken cancellationToken)
    {
        query.EnsureValid();
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMilliseconds(options.Value.TimeoutMilliseconds));
            return await read(services.GetRequiredService<IAuditRepository>(), timeout.Token);
        }
        catch (Exception)
        {
            logger.LogWarning("Falha na consulta de auditoria.");
            throw new ApiException("Auditoria temporariamente indisponível.", 503);
        }
    }
}
