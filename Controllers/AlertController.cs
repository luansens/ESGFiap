using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.ViewModels.Alert;
using ESGFiap.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertController(MongoContext mongo) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequest paging,
        [FromQuery] AlertStatus? status,
        [FromQuery] AlertSeverity? severity)
    {
        var filterBuilder = Builders<AlertDocument>.Filter;
        var filter = filterBuilder.Eq(a => a.UserId, UserId);
        if (status.HasValue) filter &= filterBuilder.Eq(a => a.Status, status.Value);
        if (severity.HasValue) filter &= filterBuilder.Eq(a => a.Severity, severity.Value);

        var total = (int)await mongo.Alerts.CountDocumentsAsync(filter);
        var items = await mongo.Alerts.Find(filter)
            .SortByDescending(a => a.DetectedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Limit(paging.PageSize)
            .ToListAsync();

        return Ok(new PagedResult<AlertViewModel>
        {
            Data = items.Select(AlertViewModel.FromModel),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var alert = await mongo.Alerts.Find(a => a.Id == id && a.UserId == UserId).FirstOrDefaultAsync();
        return alert is null ? NotFound() : Ok(AlertViewModel.FromModel(alert));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, AlertUpdateViewModel model)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var alert = await mongo.Alerts.Find(a => a.Id == id && a.UserId == UserId).FirstOrDefaultAsync();
        if (alert is null) return NotFound();

        var update = Builders<AlertDocument>.Update.Set(a => a.Status, model.Status);
        if (model.Status == AlertStatus.Resolved)
        {
            var resolvedAt = DateTime.UtcNow;
            update = update
                .Set(a => a.ResolvedAt, resolvedAt)
                .Push(a => a.ResolutionActions, new BsonDocument
                {
                    { "action", "Resolved" },
                    { "by", UserId },
                    { "at", resolvedAt }
                });
        }

        await mongo.Alerts.UpdateOneAsync(a => a.Id == id, update);
        var updated = await mongo.Alerts.Find(a => a.Id == id).FirstAsync();
        return Ok(AlertViewModel.FromModel(updated));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] PagedRequest paging,
        [FromQuery] AlertStatus? status)
    {
        var filter = status.HasValue
            ? Builders<AlertDocument>.Filter.Eq(a => a.Status, status.Value)
            : Builders<AlertDocument>.Filter.Empty;

        var total = (int)await mongo.Alerts.CountDocumentsAsync(filter);
        var items = await mongo.Alerts.Find(filter)
            .SortByDescending(a => a.DetectedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Limit(paging.PageSize)
            .ToListAsync();

        return Ok(new PagedResult<AlertViewModel>
        {
            Data = items.Select(AlertViewModel.FromModel),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }
}
