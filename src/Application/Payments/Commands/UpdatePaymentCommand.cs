using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Payments.Commands;

public record UpdatePaymentCommand : IRequest
{
    public Guid Id { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? GatewayReference { get; init; }
    public PaymentGatewayStatus? Status { get; init; }
    public string? Meta { get; init; }
    public string? IdempotencyKey { get; init; }
}

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Payments.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Payment), request.Id.ToString());
        }

        if (request.Amount.HasValue)
            entity.Amount = request.Amount.Value;

        if (request.Currency != null)
            entity.Currency = request.Currency;

        if (request.GatewayReference != null)
            entity.GatewayReference = request.GatewayReference;

        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        if (request.Meta != null)
            entity.Meta = request.Meta;

        if (request.IdempotencyKey != null)
            entity.IdempotencyKey = request.IdempotencyKey;

        _unitOfWork.Payments.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
