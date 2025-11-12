using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Domain.Entities;

public class JobApplication : BaseAuditableEntity<Guid>
{
    // Guid id will be inherited from BaseEntity<Guid>
    public Guid JobId { get; set; }
    public virtual Job? Job { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public Guid? ResumeFileId { get; set; }
    public virtual Document? ResumeFile { get; set; }

    public JobApplicationSubmissionStatus Status { get; set; } = JobApplicationSubmissionStatus.Submitted;
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
}
