namespace MigratingAssistant.Application.Jobs.Commands.UpdateJob;

public record UpdateJobCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid ListingId { get; init; }
    public string? JobType { get; init; }
    public string? Responsibilities { get; init; }
    public string? Requirements { get; init; }
}

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Jobs
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Job), request.Id.ToString());
        }

        entity.ListingId = request.ListingId;
        entity.JobType = request.JobType;
        entity.Responsibilities = request.Responsibilities;
        entity.Requirements = request.Requirements;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
