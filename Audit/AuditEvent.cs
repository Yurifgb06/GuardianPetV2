using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GuardianPet.Audit;

public sealed class AuditEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty;
    [BsonElement("entityId")]
    public long EntityId { get; set; }
    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;
    [BsonElement("occurredAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime OccurredAtUtc { get; set; }
    [BsonElement("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;
}
