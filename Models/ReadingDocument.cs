using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ESGFiap.Models;

public class ReadingDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string SensorId { get; set; } = string.Empty;

    public string SensorName { get; set; } = string.Empty;
    public decimal LitersPerHour { get; set; }
    public decimal PressureBar { get; set; }
    public DateTime ReadingAt { get; set; } = DateTime.UtcNow;

    /// Present only on some readings: anomaly/quality metadata attached by the
    /// leak-detection pipeline (status, notes, confidence).
    [BsonIgnoreIfNull]
    public BsonDocument? Quality { get; set; }
}
