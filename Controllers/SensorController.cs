using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.ViewModels.Common;
using ESGFiap.ViewModels.Sensor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SensorController(AppDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paging)
    {
        var query = db.WaterSensors.Where(s => s.UserId == UserId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(s => SensorViewModel.FromModel(s))
            .ToListAsync();

        return Ok(new PagedResult<SensorViewModel>
        {
            Data = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sensor = await db.WaterSensors
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        return sensor is null ? NotFound() : Ok(SensorViewModel.FromModel(sensor));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SensorCreateViewModel model)
    {
        var sensor = new WaterSensor
        {
            Name = model.Name,
            Location = model.Location,
            Type = model.Type,
            AlertThresholdLitersPerHour = model.AlertThresholdLitersPerHour,
            UserId = UserId
        };
        db.WaterSensors.Add(sensor);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = sensor.Id }, SensorViewModel.FromModel(sensor));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SensorCreateViewModel model)
    {
        var sensor = await db.WaterSensors.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (sensor is null) return NotFound();

        sensor.Name = model.Name;
        sensor.Location = model.Location;
        sensor.Type = model.Type;
        sensor.AlertThresholdLitersPerHour = model.AlertThresholdLitersPerHour;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var sensor = await db.WaterSensors.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (sensor is null) return NotFound();

        sensor.IsActive = !sensor.IsActive;
        await db.SaveChangesAsync();
        return Ok(new { sensor.Id, sensor.IsActive });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sensor = await db.WaterSensors.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (sensor is null) return NotFound();

        db.WaterSensors.Remove(sensor);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
