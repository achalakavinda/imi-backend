namespace MigratingAssistant.Application.Jobs.Commands.DeleteJob;

public record DeleteJobCommand(Guid Id) : IRequest;

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Jobs
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Job), request.Id.ToString());
        }

        _context.Jobs.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
