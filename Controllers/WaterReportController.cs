using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.ViewModels.Common;
using ESGFiap.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WaterReportController(AppDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paging)
    {
        var query = db.WaterImpactReports
            .Include(r => r.User)
            .Where(r => r.UserId == UserId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(r => ReportViewModel.FromModel(r))
            .ToListAsync();

        return Ok(new PagedResult<ReportViewModel>
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
        var report = await db.WaterImpactReports.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == UserId);
        return report is null ? NotFound() : Ok(ReportViewModel.FromModel(report));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReportCreateViewModel model)
    {
        var totalConsumed = await db.WaterReadings
            .Include(r => r.Sensor)
            .Where(r => r.Sensor.UserId == UserId
                     && r.ReadingAt >= model.PeriodStart
                     && r.ReadingAt <= model.PeriodEnd)
            .SumAsync(r => r.LitersPerHour);

        var report = new WaterImpactReport
        {
            Title = model.Title,
            Region = model.Region,
            PeriodStart = model.PeriodStart,
            PeriodEnd = model.PeriodEnd,
            TotalConsumedLiters = totalConsumed,
            RegionalAverageLiters = model.RegionalAverageLiters,
            ImpactAnalysis = model.ImpactAnalysis,
            MitigationPlan = model.MitigationPlan,
            UserId = UserId
        };

        db.WaterImpactReports.Add(report);
        await db.SaveChangesAsync();

        var created = await db.WaterImpactReports.Include(r => r.User)
            .FirstAsync(r => r.Id == report.Id);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, ReportViewModel.FromModel(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ReportCreateViewModel model)
    {
        var report = await db.WaterImpactReports
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == UserId);
        if (report is null) return NotFound();

        report.Title = model.Title;
        report.Region = model.Region;
        report.PeriodStart = model.PeriodStart;
        report.PeriodEnd = model.PeriodEnd;
        report.RegionalAverageLiters = model.RegionalAverageLiters;
        report.ImpactAnalysis = model.ImpactAnalysis;
        report.MitigationPlan = model.MitigationPlan;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await db.WaterImpactReports
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == UserId);
        if (report is null) return NotFound();

        db.WaterImpactReports.Remove(report);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
