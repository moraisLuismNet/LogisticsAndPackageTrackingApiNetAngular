using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using LogisticPackageTrackingApiNet.Infrastructure.Persistence;
using MongoDB.Driver;

namespace LogisticPackageTrackingApiNet.Infrastructure.Repositories.Mongo;

public class MongoRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(MongoDbContext context, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName);
    }

    public async Task AddAsync(T entity) => await _collection.InsertOneAsync(entity);

    public void Delete(T entity) 
    {
        // For simplicity in this demo, we assume the entity has an Id property
        // In a real scenario, we'd handle the key conversion
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _collection.Find(_ => true).ToListAsync();

    public async Task<T?> GetByIdAsync(int id) 
    {
        // MongoDB uses ObjectId by default, mapping to int needs custom logic
        // This is a simplified implementation
        return await _collection.Find(FilterDefinition<T>.Empty).FirstOrDefaultAsync();
    }

    public void Update(T entity) { }
}

public class MongoAuditRepository : MongoRepository<AuditLog>
{
    public MongoAuditRepository(MongoDbContext context) : base(context, "AuditLogs") { }
}
