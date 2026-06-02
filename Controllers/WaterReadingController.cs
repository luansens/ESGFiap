using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.Services;
using ESGFiap.ViewModels.Common;
using ESGFiap.ViewModels.Reading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/sensor/{sensorId:int}/readings")]
[Authorize]
public class WaterReadingController(AppDbContext db, LeakDetectionService leakService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int sensorId,
        [FromQuery] PagedRequest paging,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var owns = await db.WaterSensors.AnyAsync(s => s.Id == sensorId && s.UserId == UserId);
        if (!owns) return NotFound();

        var query = db.WaterReadings.Include(r => r.Sensor)
            .Where(r => r.SensorId == sensorId);

        if (from.HasValue) query = query.Where(r => r.ReadingAt >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.ReadingAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReadingAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(r => ReadingViewModel.FromModel(r))
            .ToListAsync();

        return Ok(new PagedResult<ReadingViewModel>
        {
            Data = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(int sensorId)
    {
        var owns = await db.WaterSensors.AnyAsync(s => s.Id == sensorId && s.UserId == UserId);
        if (!owns) return NotFound();

        var readings = await db.WaterReadings
            .Where(r => r.SensorId == sensorId)
            .ToListAsync();

        if (!readings.Any())
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
    public async Task<IActionResult> Create(int sensorId, ReadingCreateViewModel model)
    {
        var sensor = await db.WaterSensors
            .FirstOrDefaultAsync(s => s.Id == sensorId && s.UserId == UserId);
        if (sensor is null) return NotFound();
        if (!sensor.IsActive) return BadRequest("Sensor inativo.");

        var reading = new WaterReading
        {
            SensorId = sensorId,
            LitersPerHour = model.LitersPerHour,
            PressureBar = model.PressureBar,
            ReadingAt = model.ReadingAt ?? DateTime.UtcNow
        };
        db.WaterReadings.Add(reading);
        await db.SaveChangesAsync();

        var alert = await leakService.AnalyzeReading(reading, sensor);

        return CreatedAtAction(nameof(GetAll), new { sensorId },
            new { Reading = ReadingViewModel.FromModel(reading), AlertGenerated = alert is not null });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int sensorId, int id)
    {
        var owns = await db.WaterSensors.AnyAsync(s => s.Id == sensorId && s.UserId == UserId);
        if (!owns) return NotFound();

        var reading = await db.WaterReadings.FirstOrDefaultAsync(r => r.Id == id && r.SensorId == sensorId);
        if (reading is null) return NotFound();

        db.WaterReadings.Remove(reading);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
