using System.Text.Json;
using ESGFiap.Models;

namespace ESGFiap.ViewModels.Alert;

public class AlertViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal DetectedLitersPerHour { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public string SensorName { get; set; } = string.Empty;
    public string SensorLocation { get; set; } = string.Empty;
    public List<JsonElement>? ResolutionActions { get; set; }

    public static AlertViewModel FromModel(AlertDocument a) => new()
    {
        Id = a.Id,
        Message = a.Message,
        Severity = a.Severity.ToString(),
        Status = a.Status.ToString(),
        DetectedLitersPerHour = a.DetectedLitersPerHour,
        DetectedAt = a.DetectedAt,
        ResolvedAt = a.ResolvedAt,
        SensorId = a.SensorId,
        SensorName = a.SensorName,
        SensorLocation = a.SensorLocation,
        ResolutionActions = a.ResolutionActions.ToJsonElements()
    };
}
