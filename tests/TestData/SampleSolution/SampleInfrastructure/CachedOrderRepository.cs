using SampleDomain;

namespace SampleInfrastructure;

/// <summary>
/// Cached decorator for IOrderRepository. Second implementation for testing find_implementations.
/// </summary>
public class CachedOrderRepository(IOrderRepository inner) : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _cache = [];

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(id, out var cached))
            return cached;

        var order = await inner.GetByIdAsync(id, ct);
        if (order is not null)
            _cache[id] = order;

        return order;
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return inner.GetAllAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await inner.AddAsync(order, ct);
        _cache[order.Id] = order;
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _cache.Remove(order.Id);
        return inner.UpdateAsync(order, ct);
    }
}
