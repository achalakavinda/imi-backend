using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Payments.Commands;

public record DeletePaymentCommand(Guid Id) : IRequest;

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePaymentCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Payments.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Payment), request.Id.ToString());
        }

        _unitOfWork.Payments.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
