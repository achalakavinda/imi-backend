namespace MigratingAssistant.Application.ServiceTypes.Commands.UpdateServiceType;

public record UpdateServiceTypeCommand : IRequest
{
    public int Id { get; init; }
    public string ServiceKey { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? SchemaHint { get; init; }
    public bool Enabled { get; init; }
}

public class UpdateServiceTypeCommandHandler : IRequestHandler<UpdateServiceTypeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ServiceTypes
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(ServiceType), request.Id.ToString());
        }

        entity.ServiceKey = request.ServiceKey;
        entity.DisplayName = request.DisplayName;
        entity.SchemaHint = request.SchemaHint;
        entity.Enabled = request.Enabled;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
