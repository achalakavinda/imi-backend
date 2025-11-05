using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.SupportTickets.Commands;

public record DeleteSupportTicketCommand(Guid Id) : IRequest;

public class DeleteSupportTicketCommandHandler : IRequestHandler<DeleteSupportTicketCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSupportTicketCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.SupportTickets.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SupportTicket), request.Id.ToString());
        }

        _unitOfWork.SupportTickets.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}