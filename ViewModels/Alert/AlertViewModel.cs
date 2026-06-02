using ESGFiap.Models;

namespace ESGFiap.ViewModels.Alert;

public class AlertViewModel
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal DetectedLitersPerHour { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;
    public string SensorLocation { get; set; } = string.Empty;

    public static AlertViewModel FromModel(LeakAlert a) => new()
    {
        Id = a.Id,
        Message = a.Message,
        Severity = a.Severity.ToString(),
        Status = a.Status.ToString(),
        DetectedLitersPerHour = a.DetectedLitersPerHour,
        DetectedAt = a.DetectedAt,
        ResolvedAt = a.ResolvedAt,
        SensorId = a.SensorId,
        SensorName = a.Sensor?.Name ?? string.Empty,
        SensorLocation = a.Sensor?.Location ?? string.Empty
    };
}
