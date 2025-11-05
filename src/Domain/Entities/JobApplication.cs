using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class JobApplication : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid JobId { get; set; }
    public Job? Job { get; private set; }

    public Guid UserId { get; set; }
    public User? User { get; private set; }

    public Guid? ResumeFileId { get; set; }
    public Document? ResumeFile { get; private set; }

    public JobApplicationSubmissionStatus Status { get; set; } = JobApplicationSubmissionStatus.Submitted;
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
}
