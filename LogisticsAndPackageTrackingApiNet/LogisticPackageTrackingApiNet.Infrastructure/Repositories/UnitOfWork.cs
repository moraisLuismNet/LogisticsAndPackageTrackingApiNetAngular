using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using LogisticPackageTrackingApiNet.Infrastructure.Persistence;

namespace LogisticPackageTrackingApiNet.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Shipments = new ShipmentRepository(_context);
        Tracking = new TrackingRepository(_context);
        Users = new UserRepository(_context);
        AuditLogs = new Repository<AuditLog>(_context);
    }

    public IShipmentRepository Shipments { get; }
    public ITrackingRepository Tracking { get; }
    public IUserRepository Users { get; }
    public IRepository<AuditLog> AuditLogs { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
