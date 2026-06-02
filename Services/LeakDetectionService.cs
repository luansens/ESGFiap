using ESGFiap.Data;
using ESGFiap.Models;

namespace ESGFiap.Services;

public class LeakDetectionService(AppDbContext db)
{
    public async Task<LeakAlert?> AnalyzeReading(WaterReading reading, WaterSensor sensor)
    {
        if (reading.LitersPerHour <= sensor.AlertThresholdLitersPerHour)
            return null;

        var excess = reading.LitersPerHour - sensor.AlertThresholdLitersPerHour;
        var percentageOver = excess / sensor.AlertThresholdLitersPerHour * 100;

        var severity = percentageOver switch
        {
            < 25  => AlertSeverity.Low,
            < 75  => AlertSeverity.Medium,
            < 200 => AlertSeverity.High,
            _     => AlertSeverity.Critical
        };

        var alert = new LeakAlert
        {
            SensorId = sensor.Id,
            DetectedLitersPerHour = reading.LitersPerHour,
            Severity = severity,
            Message = $"Consumo de {reading.LitersPerHour:F1} L/h detectado em '{sensor.Location}'. " +
                      $"Limite configurado: {sensor.AlertThresholdLitersPerHour:F1} L/h " +
                      $"({percentageOver:F0}% acima do limite)."
        };

        db.LeakAlerts.Add(alert);
        await db.SaveChangesAsync();
        return alert;
    }
}
