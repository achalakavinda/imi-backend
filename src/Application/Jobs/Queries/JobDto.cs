namespace MigratingAssistant.Application.Jobs.Queries;

public class JobDto
{
    public Guid Id { get; init; }
    public Guid ListingId { get; init; }
    public string? JobType { get; init; }
    public string? Responsibilities { get; init; }
    public string? Requirements { get; init; }
    public DateTimeOffset PostedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Job, JobDto>();
        }
    }
}
