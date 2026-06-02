using ESGFiap.Models;

namespace ESGFiap.ViewModels.Report;

public class ReportViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalConsumedLiters { get; set; }
    public decimal RegionalAverageLiters { get; set; }
    public decimal ImpactPercentage { get; set; }
    public string ImpactAnalysis { get; set; } = string.Empty;
    public string MitigationPlan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;

    public static ReportViewModel FromModel(WaterImpactReport r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Region = r.Region,
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        TotalConsumedLiters = r.TotalConsumedLiters,
        RegionalAverageLiters = r.RegionalAverageLiters,
        ImpactPercentage = r.RegionalAverageLiters > 0
            ? Math.Round(r.TotalConsumedLiters / r.RegionalAverageLiters * 100, 2) : 0,
        ImpactAnalysis = r.ImpactAnalysis,
        MitigationPlan = r.MitigationPlan,
        CreatedAt = r.CreatedAt,
        UserName = r.User?.FullName ?? string.Empty
    };
}
