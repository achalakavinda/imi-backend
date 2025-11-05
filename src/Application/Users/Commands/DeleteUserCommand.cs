using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Context.Users
            .Include(u => u.Profile)
            .Include(u => u.ServiceProvider)
            .Include(u => u.Documents)
            .Include(u => u.SupportTickets)
            .Include(u => u.Bookings)
            .Include(u => u.JobApplications)
            .FirstOrDefaultAsync(u => u.Id.ToString() == request.Id.ToString(), cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(User), request.Id.ToString());
        }

        _unitOfWork.Users.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
