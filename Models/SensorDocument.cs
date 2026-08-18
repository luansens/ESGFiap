using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ESGFiap.Models;

public class SensorDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public SensorType Type { get; set; }

    public bool IsActive { get; set; } = true;
    public decimal AlertThresholdLitersPerHour { get; set; }
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;

    /// Flexible technical spec sheet: fields differ per SensorType
    /// (Flow: pipeDiameterMm/maxFlowLitersPerMinute, Pressure: calibrationDate/maxPressureBar,
    /// LeakDetector: sensitivityLevel/acousticRangeMeters).
    [BsonIgnoreIfNull]
    public BsonDocument? Specs { get; set; }
}

public enum SensorType { Flow = 1, Pressure = 2, LeakDetector = 3 }
