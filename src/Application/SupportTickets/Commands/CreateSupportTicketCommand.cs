using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;

namespace MigratingAssistant.Application.SupportTickets.Commands;

public record CreateSupportTicketCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
}

public class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupportTicketCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var entity = new SupportTicket
        {
            UserId = request.UserId,
            Subject = request.Subject,
            Body = request.Body,
            Status = SupportTicketStatus.Open
        };

        _unitOfWork.SupportTickets.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}