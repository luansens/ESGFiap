using ESGFiap.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ESGFiap.Data;

public class MongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        Database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<UserDocument> Users => Database.GetCollection<UserDocument>("users");
    public IMongoCollection<SensorDocument> Sensors => Database.GetCollection<SensorDocument>("sensors");
    public IMongoCollection<ReadingDocument> Readings => Database.GetCollection<ReadingDocument>("readings");
    public IMongoCollection<AlertDocument> Alerts => Database.GetCollection<AlertDocument>("alerts");
    public IMongoCollection<ReportDocument> Reports => Database.GetCollection<ReportDocument>("impact_reports");

    public async Task EnsureIndexesAsync()
    {
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(u => u.NormalizedEmail),
            new CreateIndexOptions { Unique = true, Name = "ux_users_normalizedEmail" }));

        await Sensors.Indexes.CreateOneAsync(new CreateIndexModel<SensorDocument>(
            Builders<SensorDocument>.IndexKeys.Ascending(s => s.UserId),
            new CreateIndexOptions { Name = "ix_sensors_userId" }));

        await Readings.Indexes.CreateOneAsync(new CreateIndexModel<ReadingDocument>(
            Builders<ReadingDocument>.IndexKeys.Ascending(r => r.SensorId).Descending(r => r.ReadingAt),
            new CreateIndexOptions { Name = "ix_readings_sensorId_readingAt" }));

        await Alerts.Indexes.CreateOneAsync(new CreateIndexModel<AlertDocument>(
            Builders<AlertDocument>.IndexKeys.Ascending(a => a.UserId).Descending(a => a.DetectedAt),
            new CreateIndexOptions { Name = "ix_alerts_userId_detectedAt" }));

        await Reports.Indexes.CreateOneAsync(new CreateIndexModel<ReportDocument>(
            Builders<ReportDocument>.IndexKeys.Ascending(r => r.UserId).Descending(r => r.CreatedAt),
            new CreateIndexOptions { Name = "ix_reports_userId_createdAt" }));
    }
}
