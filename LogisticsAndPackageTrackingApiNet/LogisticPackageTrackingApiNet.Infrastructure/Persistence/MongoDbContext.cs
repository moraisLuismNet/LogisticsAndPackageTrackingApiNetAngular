using LogisticPackageTrackingApiNet.Domain.Entities;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace LogisticPackageTrackingApiNet.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        _database = client.GetDatabase(configuration["MongoDb:DatabaseName"] ?? "LogisticTracking");
        
        CreateIndexes();
    }

    public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);
    public IMongoCollection<Shipment> Shipments => GetCollection<Shipment>("Shipments");
    public IMongoCollection<User> Users => GetCollection<User>("Users");
    public IMongoCollection<AuditLog> AuditLogs => GetCollection<AuditLog>("AuditLogs");

    private void CreateIndexes()
    {
        var shipmentIndexKeys = Builders<Shipment>.IndexKeys.Ascending(s => s.TrackingNumber);
        var shipmentIndexModel = new CreateIndexModel<Shipment>(shipmentIndexKeys, new CreateIndexOptions { Unique = true });
        Shipments.Indexes.CreateOne(shipmentIndexModel);

        var userIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Mail);
        var userIndexModel = new CreateIndexModel<User>(userIndexKeys, new CreateIndexOptions { Unique = true });
        Users.Indexes.CreateOne(userIndexModel);
    }
}
