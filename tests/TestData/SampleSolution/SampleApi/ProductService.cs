using SampleDomain;

namespace SampleApi;

/// <summary>
/// Application service for product operations.
/// Intentionally has a diagnostic: unused variable for testing get_diagnostics.
/// </summary>
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken ct = default)
    {
        var result = await _repository.GetAllAsync(ct);
        // Intentional unused variable — generates CS0219 diagnostic for testing
#pragma warning disable CS0219
        int unusedVariable = 42;
#pragma warning restore CS0219
        return result;
    }
}
