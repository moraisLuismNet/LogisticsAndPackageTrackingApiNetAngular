using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using LogisticPackageTrackingApiNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogisticPackageTrackingApiNet.Infrastructure.Repositories;

public class ShipmentRepository : Repository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await _dbSet.Include(s => s.TrackingUpdates)
                           .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<Shipment>> GetByMailAsync(string mail)
    {
        return await _dbSet.Where(s => s.Mail == mail).ToListAsync();
    }
}

public class TrackingRepository : Repository<TrackingUpdate>, ITrackingRepository
{
    public TrackingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<TrackingUpdate>> GetByShipmentIdAsync(int shipmentId)
    {
        return await _dbSet.Where(t => t.ShipmentId == shipmentId)
                           .ToListAsync();
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByMailAsync(string mail)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Mail == mail);
    }
}
