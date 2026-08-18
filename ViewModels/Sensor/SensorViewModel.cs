using System.Text.Json;
using ESGFiap.Models;

namespace ESGFiap.ViewModels.Sensor;

public class SensorViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal AlertThresholdLitersPerHour { get; set; }
    public DateTime InstalledAt { get; set; }
    public JsonElement? Specs { get; set; }

    public static SensorViewModel FromModel(SensorDocument s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Location = s.Location,
        Type = s.Type.ToString(),
        IsActive = s.IsActive,
        AlertThresholdLitersPerHour = s.AlertThresholdLitersPerHour,
        InstalledAt = s.InstalledAt,
        Specs = s.Specs.ToJsonElement()
    };
}
