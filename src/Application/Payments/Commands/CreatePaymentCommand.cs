using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Payments.Commands;

public record CreatePaymentCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AUD";
    public string? GatewayReference { get; init; }
    public PaymentGatewayStatus Status { get; init; }
    public string? Meta { get; init; } // JSON
    public string? IdempotencyKey { get; init; }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePaymentCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        var entity = new Payment
        {
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            GatewayReference = request.GatewayReference,
            Status = request.Status,
            Meta = request.Meta,
            IdempotencyKey = request.IdempotencyKey
        };

        _unitOfWork.Payments.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
