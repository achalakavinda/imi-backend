namespace MigratingAssistant.Application.ServiceTypes.Commands.CreateServiceType;

public record CreateServiceTypeCommand : IRequest<int>
{
    public string ServiceKey { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? SchemaHint { get; init; }
    public bool Enabled { get; init; } = true;
}

public class CreateServiceTypeCommandHandler : IRequestHandler<CreateServiceTypeCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateServiceTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = new ServiceType
        {
            ServiceKey = request.ServiceKey,
            DisplayName = request.DisplayName,
            SchemaHint = request.SchemaHint,
            Enabled = request.Enabled
        };

        _context.ServiceTypes.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
