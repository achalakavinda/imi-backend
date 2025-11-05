using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Bookings.Commands;

public record CreateBookingCommand : IRequest<Guid>
{
    public Guid ListingId { get; init; }
    public Guid? InventoryItemId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
}

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists using repository
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        // Validate listing exists using repository
        var listing = await _unitOfWork.Listings.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing == null)
        {
            throw new NotFoundException(nameof(Listing), request.ListingId.ToString());
        }

        // Validate inventory item if provided
        if (request.InventoryItemId.HasValue)
        {
            var inventoryItem = await _unitOfWork.InventoryItems.GetByIdAsync(request.InventoryItemId.Value, cancellationToken);
            if (inventoryItem == null)
            {
                throw new NotFoundException(nameof(InventoryItem), request.InventoryItemId.Value.ToString());
            }
        }

        // Create booking entity
        var entity = new Booking
        {
            ListingId = request.ListingId,
            InventoryItemId = request.InventoryItemId,
            UserId = request.UserId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Status = BookingStatus.Pending
        };

        // Insert using repository
        _unitOfWork.Bookings.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
