using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.ServiceProviders.Commands;

public record DeleteServiceProviderCommand(Guid Id) : IRequest;

public class DeleteServiceProviderCommandHandler : IRequestHandler<DeleteServiceProviderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteServiceProviderCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteServiceProviderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Context.ServiceProviders
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(ServiceProvider), request.Id.ToString());
        }

        _unitOfWork.ServiceProviders.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
