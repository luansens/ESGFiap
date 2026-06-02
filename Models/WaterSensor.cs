namespace ESGFiap.Models;

public class WaterSensor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal AlertThresholdLitersPerHour { get; set; }
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<WaterReading> Readings { get; set; } = [];
    public ICollection<LeakAlert> Alerts { get; set; } = [];
}

public enum SensorType { Flow = 1, Pressure = 2, LeakDetector = 3 }
