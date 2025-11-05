using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Documents.Commands;

public record DeleteDocumentCommand(Guid Id) : IRequest;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new MigratingAssistant.Application.Common.Exceptions.NotFoundException(nameof(Document), request.Id.ToString());
        }

        _unitOfWork.Documents.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
