using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.JobApplications.Queries;

public class JobApplicationDto
{
    public Guid Id { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Status { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string? Notes { get; set; }
    public Guid UserId { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<JobApplication, JobApplicationDto>();
        }
    }
}