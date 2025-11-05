using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Bookings.Commands;

public record UpdateBookingCommand : IRequest
{
    public Guid Id { get; init; }
    public DateTimeOffset? StartAt { get; init; }
    public DateTimeOffset? EndAt { get; init; }
    public BookingStatus? Status { get; init; }
}

public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        // Get entity using repository
        var entity = await _unitOfWork.Bookings.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException(nameof(Booking), request.Id.ToString());
        }

        // Update properties
        if (request.StartAt.HasValue)
            entity.StartAt = request.StartAt.Value;

        if (request.EndAt.HasValue)
            entity.EndAt = request.EndAt.Value;

        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        // Mark entity as modified using repository
        _unitOfWork.Bookings.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
