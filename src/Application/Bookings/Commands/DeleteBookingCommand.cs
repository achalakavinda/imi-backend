using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Bookings.Commands;

public record DeleteBookingCommand(Guid Id) : IRequest;

public class DeleteBookingCommandHandler : IRequestHandler<DeleteBookingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        // Get entity with related data using Context (for complex queries with includes)
        var entity = await _unitOfWork.Context.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Booking), request.Id.ToString());
        }

        // Delete using repository
        _unitOfWork.Bookings.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
