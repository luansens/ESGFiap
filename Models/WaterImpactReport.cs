namespace ESGFiap.Models;

public class WaterImpactReport
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalConsumedLiters { get; set; }
    public decimal RegionalAverageLiters { get; set; }
    public string ImpactAnalysis { get; set; } = string.Empty;
    public string MitigationPlan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
