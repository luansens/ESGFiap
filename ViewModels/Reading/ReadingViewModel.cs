using System.Text.Json;
using ESGFiap.Models;

namespace ESGFiap.ViewModels.Reading;

public class ReadingViewModel
{
    public string Id { get; set; } = string.Empty;
    public decimal LitersPerHour { get; set; }
    public decimal PressureBar { get; set; }
    public DateTime ReadingAt { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public string SensorName { get; set; } = string.Empty;
    public JsonElement? Quality { get; set; }

    public static ReadingViewModel FromModel(ReadingDocument r) => new()
    {
        Id = r.Id,
        LitersPerHour = r.LitersPerHour,
        PressureBar = r.PressureBar,
        ReadingAt = r.ReadingAt,
        SensorId = r.SensorId,
        SensorName = r.SensorName,
        Quality = r.Quality.ToJsonElement()
    };
}
