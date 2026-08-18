using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.Services;
using ESGFiap.ViewModels.Common;
using ESGFiap.ViewModels.Reading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/sensor/{sensorId}/readings")]
[Authorize]
public class WaterReadingController(MongoContext mongo, LeakDetectionService leakService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string sensorId,
        [FromQuery] PagedRequest paging,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (!sensorId.IsValidObjectId()) return NotFound();

        var owns = await mongo.Sensors.Find(s => s.Id == sensorId && s.UserId == UserId).AnyAsync();
        if (!owns) return NotFound();

        var filterBuilder = Builders<ReadingDocument>.Filter;
        var filter = filterBuilder.Eq(r => r.SensorId, sensorId);
        if (from.HasValue) filter &= filterBuilder.Gte(r => r.ReadingAt, from.Value);
        if (to.HasValue) filter &= filterBuilder.Lte(r => r.ReadingAt, to.Value);

        var total = (int)await mongo.Readings.CountDocumentsAsync(filter);
        var items = await mongo.Readings.Find(filter)
            .SortByDescending(r => r.ReadingAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Limit(paging.PageSize)
            .ToListAsync();

        return Ok(new PagedResult<ReadingViewModel>
        {
            Data = items.Select(ReadingViewModel.FromModel),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(string sensorId)
    {
        if (!sensorId.IsValidObjectId()) return NotFound();

        var owns = await mongo.Sensors.Find(s => s.Id == sensorId && s.UserId == UserId).AnyAsync();
        if (!owns) return NotFound();

        var readings = await mongo.Readings.Find(r => r.SensorId == sensorId).ToListAsync();

        if (readings.Count == 0)
            return Ok(new { message = "Sem leituras registradas." });

        return Ok(new
        {
            TotalReadings = readings.Count,
            AverageLitersPerHour = Math.Round(readings.Average(r => r.LitersPerHour), 2),
            MaxLitersPerHour = readings.Max(r => r.LitersPerHour),
            MinLitersPerHour = readings.Min(r => r.LitersPerHour),
            LastReadingAt = readings.Max(r => r.ReadingAt)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string sensorId, ReadingCreateViewModel model)
    {
        if (!sensorId.IsValidObjectId()) return NotFound();

        var sensor = await mongo.Sensors.Find(s => s.Id == sensorId && s.UserId == UserId).FirstOrDefaultAsync();
        if (sensor is null) return NotFound();
        if (!sensor.IsActive) return BadRequest("Sensor inativo.");

        var reading = new ReadingDocument
        {
            SensorId = sensorId,
            SensorName = sensor.Name,
            LitersPerHour = model.LitersPerHour,
            PressureBar = model.PressureBar,
            ReadingAt = model.ReadingAt ?? DateTime.UtcNow,
            Quality = model.Quality.ToBsonDocument()
        };
        await mongo.Readings.InsertOneAsync(reading);

        var alert = await leakService.AnalyzeReading(reading, sensor);

        return CreatedAtAction(nameof(GetAll), new { sensorId },
            new { Reading = ReadingViewModel.FromModel(reading), AlertGenerated = alert is not null });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string sensorId, string id)
    {
        if (!sensorId.IsValidObjectId() || !id.IsValidObjectId()) return NotFound();

        var owns = await mongo.Sensors.Find(s => s.Id == sensorId && s.UserId == UserId).AnyAsync();
        if (!owns) return NotFound();

        var result = await mongo.Readings.DeleteOneAsync(r => r.Id == id && r.SensorId == sensorId);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
