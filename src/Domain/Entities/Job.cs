namespace MigratingAssistant.Domain.Entities;

public class Job : BaseEntity<Guid>
{
    public Guid ListingId { get; set; }
    public Listing? Listing { get; private set; }

    public string? JobType { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public IList<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();
}
