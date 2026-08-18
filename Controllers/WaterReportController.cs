using System.Security.Claims;
using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.ViewModels.Common;
using ESGFiap.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WaterReportController(MongoContext mongo) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paging)
    {
        var filter = Builders<ReportDocument>.Filter.Eq(r => r.UserId, UserId);

        var total = (int)await mongo.Reports.CountDocumentsAsync(filter);
        var items = await mongo.Reports.Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Limit(paging.PageSize)
            .ToListAsync();

        return Ok(new PagedResult<ReportViewModel>
        {
            Data = items.Select(ReportViewModel.FromModel),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var report = await mongo.Reports.Find(r => r.Id == id && r.UserId == UserId).FirstOrDefaultAsync();
        return report is null ? NotFound() : Ok(ReportViewModel.FromModel(report));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReportCreateViewModel model)
    {
        var sensorIds = await mongo.Sensors.Find(s => s.UserId == UserId)
            .Project(s => s.Id)
            .ToListAsync();

        var readingsFilter = Builders<ReadingDocument>.Filter.In(r => r.SensorId, sensorIds)
            & Builders<ReadingDocument>.Filter.Gte(r => r.ReadingAt, model.PeriodStart)
            & Builders<ReadingDocument>.Filter.Lte(r => r.ReadingAt, model.PeriodEnd);

        var totalConsumed = await mongo.Readings.Find(readingsFilter).ToListAsync();

        var user = await mongo.Users.Find(u => u.Id == UserId).FirstAsync();

        var report = new ReportDocument
        {
            Title = model.Title,
            Region = model.Region,
            PeriodStart = model.PeriodStart,
            PeriodEnd = model.PeriodEnd,
            TotalConsumedLiters = totalConsumed.Sum(r => r.LitersPerHour),
            RegionalAverageLiters = model.RegionalAverageLiters,
            ImpactAnalysis = model.ImpactAnalysis,
            MitigationPlan = model.MitigationPlan,
            ReportType = model.ReportType,
            EsgMetrics = model.EsgMetrics.ToBsonDocument(),
            UserId = UserId,
            UserName = user.FullName
        };

        await mongo.Reports.InsertOneAsync(report);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, ReportViewModel.FromModel(report));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, ReportCreateViewModel model)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var update = Builders<ReportDocument>.Update
            .Set(r => r.Title, model.Title)
            .Set(r => r.Region, model.Region)
            .Set(r => r.PeriodStart, model.PeriodStart)
            .Set(r => r.PeriodEnd, model.PeriodEnd)
            .Set(r => r.RegionalAverageLiters, model.RegionalAverageLiters)
            .Set(r => r.ImpactAnalysis, model.ImpactAnalysis)
            .Set(r => r.MitigationPlan, model.MitigationPlan)
            .Set(r => r.ReportType, model.ReportType)
            .Set(r => r.EsgMetrics, model.EsgMetrics.ToBsonDocument());

        var result = await mongo.Reports.UpdateOneAsync(
            r => r.Id == id && r.UserId == UserId, update);

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!id.IsValidObjectId()) return NotFound();

        var result = await mongo.Reports.DeleteOneAsync(r => r.Id == id && r.UserId == UserId);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
