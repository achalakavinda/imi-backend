using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.SupportTickets.Commands;

public record UpdateSupportTicketCommand : IRequest
{
    public Guid Id { get; init; }
    public string? Subject { get; init; }
    public string? Body { get; init; }
    public SupportTicketStatus? Status { get; init; }
}

public class UpdateSupportTicketCommandHandler : IRequestHandler<UpdateSupportTicketCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupportTicketCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.SupportTickets.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SupportTicket), request.Id.ToString());
        }

        if (request.Subject != null)
            entity.Subject = request.Subject;

        if (request.Body != null)
            entity.Body = request.Body;

        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        _unitOfWork.SupportTickets.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}