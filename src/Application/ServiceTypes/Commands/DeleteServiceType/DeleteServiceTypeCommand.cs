namespace MigratingAssistant.Application.ServiceTypes.Commands.DeleteServiceType;

public record DeleteServiceTypeCommand(int Id) : IRequest;

public class DeleteServiceTypeCommandHandler : IRequestHandler<DeleteServiceTypeCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteServiceTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ServiceTypes
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ServiceType), request.Id.ToString());
        }

        _context.ServiceTypes.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
