using ESGFiap.Models;

namespace ESGFiap.ViewModels.Reading;

public class ReadingViewModel
{
    public int Id { get; set; }
    public decimal LitersPerHour { get; set; }
    public decimal PressureBar { get; set; }
    public DateTime ReadingAt { get; set; }
    public int SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;

    public static ReadingViewModel FromModel(WaterReading r) => new()
    {
        Id = r.Id,
        LitersPerHour = r.LitersPerHour,
        PressureBar = r.PressureBar,
        ReadingAt = r.ReadingAt,
        SensorId = r.SensorId,
        SensorName = r.Sensor?.Name ?? string.Empty
    };
}
