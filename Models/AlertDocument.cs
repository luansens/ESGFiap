using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ESGFiap.Models;

public class AlertDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string SensorId { get; set; } = string.Empty;

    public string SensorName { get; set; } = string.Empty;
    public string SensorLocation { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public AlertSeverity Severity { get; set; }

    [BsonRepresentation(BsonType.String)]
    public AlertStatus Status { get; set; } = AlertStatus.Open;

    public decimal DetectedLitersPerHour { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    /// Present only once an alert has been worked: audit trail of actions taken
    /// (who acknowledged/resolved it, when, and what mitigation was applied).
    [BsonIgnoreIfNull]
    public List<BsonDocument>? ResolutionActions { get; set; }
}

public enum AlertSeverity { Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum AlertStatus { Open = 1, Acknowledged = 2, Resolved = 3 }
