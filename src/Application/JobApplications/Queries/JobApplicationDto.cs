using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Application.Jobs.Queries;
using MigratingAssistant.Application.Documents.Queries;

namespace MigratingAssistant.Application.JobApplications.Queries;

public class JobApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ResumeFileId { get; set; }
    public int Status { get; set; }
    public DateTimeOffset AppliedAt { get; set; }

    // Nested entities
    public JobDto? Job { get; set; }
    public DocumentDto? ResumeFile { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<JobApplication, JobApplicationDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status));
        }
    }
}