using SampleDomain;

namespace SampleInfrastructure;

/// <summary>
/// In-memory implementation of IProductRepository.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = [];

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _products.AsReadOnly();
        return Task.FromResult(result);
    }
}
