using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.ViewModels.Common;
using ESGFiap.ViewModels.Sensor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SensorController(MongoContext mongo) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paging)
    {
        var filter = Builders<SensorDocument>.Filter.Eq(s => s.UserId, UserId);

        var total = (int)await mongo.Sensors.CountDocumentsAsync(filter);
        var items = await mongo.Sensors.Find(filter)
            .SortBy(s => s.Name)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Limit(paging.PageSize)
            .ToListAsync();

        return Ok(new PagedResult<SensorViewModel>
        {
            Data = items.Select(SensorViewModel.FromModel),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var sensor = await mongo.Sensors.Find(s => s.Id == id && s.UserId == UserId).FirstOrDefaultAsync();
        return sensor is null ? NotFound() : Ok(SensorViewModel.FromModel(sensor));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SensorCreateViewModel model)
    {
        var sensor = new SensorDocument
        {
            Name = model.Name,
            Location = model.Location,
            Type = model.Type,
            AlertThresholdLitersPerHour = model.AlertThresholdLitersPerHour,
            UserId = UserId,
            Specs = model.Specs.ToBsonDocument()
        };
        await mongo.Sensors.InsertOneAsync(sensor);
        return CreatedAtAction(nameof(GetById), new { id = sensor.Id }, SensorViewModel.FromModel(sensor));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, SensorCreateViewModel model)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var update = Builders<SensorDocument>.Update
            .Set(s => s.Name, model.Name)
            .Set(s => s.Location, model.Location)
            .Set(s => s.Type, model.Type)
            .Set(s => s.AlertThresholdLitersPerHour, model.AlertThresholdLitersPerHour)
            .Set(s => s.Specs, model.Specs.ToBsonDocument());

        var result = await mongo.Sensors.UpdateOneAsync(
            s => s.Id == id && s.UserId == UserId, update);

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(string id)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var sensor = await mongo.Sensors.Find(s => s.Id == id && s.UserId == UserId).FirstOrDefaultAsync();
        if (sensor is null) return NotFound();

        var update = Builders<SensorDocument>.Update.Set(s => s.IsActive, !sensor.IsActive);
        await mongo.Sensors.UpdateOneAsync(s => s.Id == id, update);

        return Ok(new { sensor.Id, IsActive = !sensor.IsActive });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var result = await mongo.Sensors.DeleteOneAsync(s => s.Id == id && s.UserId == UserId);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
