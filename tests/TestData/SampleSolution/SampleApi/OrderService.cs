using SampleDomain;

namespace SampleApi;

/// <summary>
/// Application service for order operations. Cross-project reference to domain and infrastructure.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Order?> GetOrderAsync(Guid id, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(id, ct);
    }

    public async Task<Order> CreateOrderAsync(string customerId, List<OrderItem> items, CancellationToken ct = default)
    {
        var order = Order.Create(customerId, items, DateTime.UtcNow);
        await _repository.AddAsync(order, ct);
        return order;
    }

    public async Task CancelOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Order {id} not found");
        order.Cancel();
        await _repository.UpdateAsync(order, ct);
    }
}
