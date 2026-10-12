using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GuardianPet.Audit;

public sealed class MongoAuditRepository(IMongoClient client, IOptions<MongoAuditOptions> options) : IAuditRepository
{
    private IMongoCollection<AuditEvent> Collection =>
        client.GetDatabase(options.Value.DatabaseName).GetCollection<AuditEvent>(options.Value.CollectionName);

    public Task InsertAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
        => Collection.InsertOneAsync(auditEvent, cancellationToken: cancellationToken);

    public Task<long> CountAsync(AuditQuery query, CancellationToken cancellationToken)
    {
        query.EnsureValid();
        return Collection.CountDocumentsAsync(BuildFilter(query), cancellationToken: cancellationToken);
    }

    public Task<List<AuditEvent>> QueryAsync(AuditQuery query, CancellationToken cancellationToken)
    {
        query.EnsureValid();
        return Collection.Find(BuildFilter(query))
            .SortByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id)
            .Skip(checked((query.Page - 1) * query.PageSize)).Limit(query.PageSize)
            .ToListAsync(cancellationToken);
    }

    public static FilterDefinition<AuditEvent> BuildFilter(AuditQuery query)
    {
        var builder = Builders<AuditEvent>.Filter;
        var filter = builder.Empty;
        if (query.EntityType is not null) filter &= builder.Eq(x => x.EntityType, query.EntityType);
        if (query.EntityId.HasValue) filter &= builder.Eq(x => x.EntityId, query.EntityId.Value);
        if (query.Action is not null) filter &= builder.Eq(x => x.Action, query.Action);
        if (query.CorrelationId is not null) filter &= builder.Eq(x => x.CorrelationId, query.CorrelationId);
        if (query.FromUtc.HasValue) filter &= builder.Gte(x => x.OccurredAtUtc, query.FromUtc.Value.ToUniversalTime());
        if (query.ToUtc.HasValue) filter &= builder.Lte(x => x.OccurredAtUtc, query.ToUtc.Value.ToUniversalTime());
        return filter;
    }
}
