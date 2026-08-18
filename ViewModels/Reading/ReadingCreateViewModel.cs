using System.Text.Json;

namespace ESGFiap.ViewModels.Reading;

public class ReadingCreateViewModel
{
    public decimal LitersPerHour { get; set; }
    public decimal PressureBar { get; set; }
    public DateTime? ReadingAt { get; set; }
    public JsonElement? Quality { get; set; }
}
