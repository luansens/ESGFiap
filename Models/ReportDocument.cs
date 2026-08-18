using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ESGFiap.Models;

public class ReportDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalConsumedLiters { get; set; }
    public decimal RegionalAverageLiters { get; set; }
    public string ImpactAnalysis { get; set; } = string.Empty;
    public string MitigationPlan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// Drives which optional sections exist in EsgMetrics below.
    public string ReportType { get; set; } = "WaterImpact";

    /// Flexible, polymorphic ESG breakdown: a "WaterImpact" report only carries
    /// environmental figures, while "ComplianceAudit"/"CommunityEngagement" reports
    /// add nested social/governance sections (e.g. socialImpact, governance).
    [BsonIgnoreIfNull]
    public BsonDocument? EsgMetrics { get; set; }
}
