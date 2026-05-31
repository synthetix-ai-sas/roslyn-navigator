namespace SampleDomain;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];

    private Order() { }

    public static Order Create(string customerId, List<OrderItem> items, DateTime createdAt)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = items,
            Total = items.Sum(i => i.Price * i.Quantity),
            Status = OrderStatus.Pending,
            CreatedAt = createdAt
        };
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel a shipped order.");
        Status = OrderStatus.Cancelled;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be shipped.");
        Status = OrderStatus.Shipped;
    }
}
