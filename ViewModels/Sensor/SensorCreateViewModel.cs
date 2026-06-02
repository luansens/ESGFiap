using ESGFiap.Models;

namespace ESGFiap.ViewModels.Sensor;

public class SensorCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public decimal AlertThresholdLitersPerHour { get; set; }
}
