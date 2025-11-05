using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.ServiceProviders.Commands;

public record UpdateServiceProviderCommand : IRequest
{
    public Guid Id { get; init; }
    public string? ProviderName { get; init; }
    public string? ProviderType { get; init; }
    public bool? Verified { get; init; }
    public string? ProviderMetadata { get; init; }
}

public class UpdateServiceProviderCommandHandler : IRequestHandler<UpdateServiceProviderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceProviderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateServiceProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.ServiceProviders.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(ServiceProvider), request.Id.ToString());
        }

        if (request.ProviderName != null)
            entity.ProviderName = request.ProviderName;

        if (request.ProviderType != null)
            entity.ProviderType = request.ProviderType;

        if (request.Verified.HasValue)
            entity.Verified = request.Verified.Value;

        if (request.ProviderMetadata != null)
            entity.ProviderMetadata = request.ProviderMetadata;

        _unitOfWork.ServiceProviders.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
