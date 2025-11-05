namespace MigratingAssistant.Domain.Entities;

public class ServiceProvider : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? ProviderName { get; set; }
    public string? ProviderType { get; set; }
    public bool Verified { get; set; } = false;
    public string? ProviderMetadata { get; set; } // JSON as string

    public IList<Listing> Listings { get; private set; } = new List<Listing>();
}
