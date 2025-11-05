namespace MigratingAssistant.Domain.Entities;

public class ProfileObsolete : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
}
