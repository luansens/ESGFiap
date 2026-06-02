namespace ESGFiap.Models;

public class LeakAlert
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Open;
    public decimal DetectedLitersPerHour { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public int SensorId { get; set; }
    public WaterSensor Sensor { get; set; } = null!;
}

public enum AlertSeverity { Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum AlertStatus { Open = 1, Acknowledged = 2, Resolved = 3 }
