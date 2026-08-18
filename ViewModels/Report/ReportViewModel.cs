using System.Text.Json;
using ESGFiap.Models;

namespace ESGFiap.ViewModels.Report;

public class ReportViewModel
{
    public string Id { get; set; } = string.Empty;
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
    public string ReportType { get; set; } = string.Empty;
    public JsonElement? EsgMetrics { get; set; }

    public static ReportViewModel FromModel(ReportDocument r) => new()
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
        UserName = r.UserName,
        ReportType = r.ReportType,
        EsgMetrics = r.EsgMetrics.ToJsonElement()
    };
}
