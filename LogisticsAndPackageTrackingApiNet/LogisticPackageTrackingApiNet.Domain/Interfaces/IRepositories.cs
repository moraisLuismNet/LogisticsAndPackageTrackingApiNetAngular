using LogisticPackageTrackingApiNet.Domain.Entities;

namespace LogisticPackageTrackingApiNet.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IShipmentRepository : IRepository<Shipment>
{
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<Shipment>> GetByMailAsync(string mail);
}

public interface ITrackingRepository : IRepository<TrackingUpdate>
{
    Task<IEnumerable<TrackingUpdate>> GetByShipmentIdAsync(int shipmentId);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByMailAsync(string mail);
}

public interface IUnitOfWork : IDisposable
{
    IShipmentRepository Shipments { get; }
    ITrackingRepository Tracking { get; }
    IUserRepository Users { get; }
    IRepository<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync();
}
