using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.ServiceProviders.Commands;

public record CreateServiceProviderCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public bool Verified { get; init; }
    public string? ProviderMetadata { get; init; }
}

public class CreateServiceProviderCommandHandler : IRequestHandler<CreateServiceProviderCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceProviderCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateServiceProviderCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        var entity = new ServiceProvider
        {
            UserId = request.UserId,
            ProviderName = request.ProviderName,
            ProviderType = request.ProviderType,
            Verified = request.Verified,
            ProviderMetadata = request.ProviderMetadata
        };

        _unitOfWork.ServiceProviders.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
