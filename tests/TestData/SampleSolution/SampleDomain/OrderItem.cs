namespace SampleDomain;

/// <summary>
/// Represents a line item in an order.
/// </summary>
public record OrderItem(string ProductName, int Quantity, decimal Price);
