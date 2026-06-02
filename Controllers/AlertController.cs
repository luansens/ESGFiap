using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.ViewModels.Alert;
using ESGFiap.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertController(AppDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequest paging,
        [FromQuery] AlertStatus? status,
        [FromQuery] AlertSeverity? severity)
    {
        var query = db.LeakAlerts
            .Include(a => a.Sensor)
            .Where(a => a.Sensor.UserId == UserId);

        if (status.HasValue)   query = query.Where(a => a.Status == status.Value);
        if (severity.HasValue) query = query.Where(a => a.Severity == severity.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.DetectedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(a => AlertViewModel.FromModel(a))
            .ToListAsync();

        return Ok(new PagedResult<AlertViewModel>
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
        var alert = await db.LeakAlerts.Include(a => a.Sensor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Sensor.UserId == UserId);
        return alert is null ? NotFound() : Ok(AlertViewModel.FromModel(alert));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, AlertUpdateViewModel model)
    {
        var alert = await db.LeakAlerts.Include(a => a.Sensor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Sensor.UserId == UserId);
        if (alert is null) return NotFound();

        alert.Status = model.Status;
        if (model.Status == AlertStatus.Resolved)
            alert.ResolvedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(AlertViewModel.FromModel(alert));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] PagedRequest paging,
        [FromQuery] AlertStatus? status)
    {
        var query = db.LeakAlerts.Include(a => a.Sensor).AsQueryable();
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.DetectedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(a => AlertViewModel.FromModel(a))
            .ToListAsync();

        return Ok(new PagedResult<AlertViewModel>
        {
            Data = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }
}
