namespace MigratingAssistant.Application.Jobs.Commands.CreateJob;

public record CreateJobCommand : IRequest<Guid>
{
    public Guid ListingId { get; init; }
    public string? JobType { get; init; }
    public string? Responsibilities { get; init; }
    public string? Requirements { get; init; }
}

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var entity = new Job
        {
            ListingId = request.ListingId,
            JobType = request.JobType,
            Responsibilities = request.Responsibilities,
            Requirements = request.Requirements,
            PostedAt = DateTimeOffset.UtcNow
        };

        _context.Jobs.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
