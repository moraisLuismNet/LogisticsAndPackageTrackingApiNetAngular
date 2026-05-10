using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using LogisticPackageTrackingApiNet.Infrastructure.Persistence;
using MongoDB.Driver;

namespace LogisticPackageTrackingApiNet.Infrastructure.Repositories.Mongo;

public class MongoUnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;

    public MongoUnitOfWork(MongoDbContext context)
    {
        _context = context;
        Shipments = new MongoShipmentRepository(_context);
        Tracking = new MongoTrackingRepository(_context);
        Users = new MongoUserRepository(_context);
        AuditLogs = new MongoAuditRepository(_context);
    }

    public IShipmentRepository Shipments { get; }
    public ITrackingRepository Tracking { get; }
    public IUserRepository Users { get; }
    public IRepository<AuditLog> AuditLogs { get; }

    public async Task<int> SaveChangesAsync()
    {
        // MongoDB inserts are immediate in this simple repo implementation
        return await Task.FromResult(1);
    }

    public void Dispose() { }
}

public class MongoShipmentRepository : MongoRepository<Shipment>, IShipmentRepository
{
    public MongoShipmentRepository(MongoDbContext context) : base(context, "Shipments") { }
    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber) 
        => await _collection.Find(s => s.TrackingNumber == trackingNumber).FirstOrDefaultAsync();
    public async Task<IEnumerable<Shipment>> GetByMailAsync(string mail)
        => await _collection.Find(s => s.Mail == mail).ToListAsync();
}

public class MongoTrackingRepository : MongoRepository<TrackingUpdate>, ITrackingRepository
{
    public MongoTrackingRepository(MongoDbContext context) : base(context, "TrackingUpdates") { }
    public async Task<IEnumerable<TrackingUpdate>> GetByShipmentIdAsync(int shipmentId)
        => await _collection.Find(t => t.ShipmentId == shipmentId).ToListAsync();
}

public class MongoUserRepository : MongoRepository<User>, IUserRepository
{
    public MongoUserRepository(MongoDbContext context) : base(context, "Users") { }
    public async Task<User?> GetByMailAsync(string mail)
        => await _collection.Find(u => u.Mail == mail).FirstOrDefaultAsync();
}
