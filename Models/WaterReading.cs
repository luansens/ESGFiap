namespace ESGFiap.Models;

public class WaterReading
{
    public int Id { get; set; }
    public decimal LitersPerHour { get; set; }
    public decimal PressureBar { get; set; }
    public DateTime ReadingAt { get; set; } = DateTime.UtcNow;

    public int SensorId { get; set; }
    public WaterSensor Sensor { get; set; } = null!;
}
