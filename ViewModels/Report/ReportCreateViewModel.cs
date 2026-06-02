namespace ESGFiap.ViewModels.Report;

public class ReportCreateViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal RegionalAverageLiters { get; set; }
    public string ImpactAnalysis { get; set; } = string.Empty;
    public string MitigationPlan { get; set; } = string.Empty;
}
