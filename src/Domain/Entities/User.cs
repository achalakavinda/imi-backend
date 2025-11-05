namespace MigratingAssistant.Domain.Entities;

public class User : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public UserRole Role { get; set; } = UserRole.User;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool EmailVerified { get; set; } = false;
    public int Version { get; set; } = 1;

    // Navigation property
    public UserProfile? Profile { get; private set; }
    public ServiceProvider? ServiceProvider { get; private set; }
    public IList<Document> Documents { get; private set; } = new List<Document>();
    public IList<SupportTicket> SupportTickets { get; private set; } = new List<SupportTicket>();
    public IList<Booking> Bookings { get; private set; } = new List<Booking>();
    public IList<Payment> Payments { get; private set; } = new List<Payment>();
    public IList<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();
}
