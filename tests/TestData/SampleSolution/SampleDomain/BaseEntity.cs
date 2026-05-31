namespace SampleDomain;

/// <summary>
/// Base class for auditable entities. Used to test type hierarchy queries.
/// </summary>
public abstract class BaseEntity
{
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Auditable product that extends BaseEntity. Tests inheritance chain.
/// </summary>
public class AuditableProduct : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
