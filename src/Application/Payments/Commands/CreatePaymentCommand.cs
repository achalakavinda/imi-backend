using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Payments.Commands;

public record CreatePaymentCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public Guid? BookingId { get; init; }
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
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        // Validate booking if provided
        Booking? booking = null;
        if (request.BookingId.HasValue)
        {
            booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId.Value, cancellationToken);
            if (booking == null)
            {
                throw new NotFoundException(nameof(Booking), request.BookingId.Value.ToString());
            }

            // Verify booking belongs to the user
            if (booking.UserId != request.UserId)
            {
                throw new InvalidOperationException("Booking does not belong to the specified user");
            }
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

        // Update booking with payment ID if booking was provided
        if (booking != null)
        {
            booking.PaymentId = entity.Id;
            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return entity.Id;
    }
}
