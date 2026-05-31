using SampleDomain;

namespace SampleInfrastructure;

/// <summary>
/// In-memory implementation of IOrderRepository.
/// </summary>
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Order> result = _orders.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        // In-memory: already updated by reference
        return Task.CompletedTask;
    }
}
