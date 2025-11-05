namespace MigratingAssistant.Domain.Entities;

public class UserProfile : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Nationality { get; set; }
    public string? Bio { get; set; }
    public string? Preferences { get; set; } // JSON as string
}
