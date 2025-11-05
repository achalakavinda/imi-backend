using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Documents.Commands;

public record UpdateDocumentCommand : IRequest
{
    public Guid Id { get; init; }
    public string? DocType { get; init; }
    public string? StoragePath { get; init; }
    public bool? Verified { get; init; }
}

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Document), request.Id.ToString());
        }

        if (request.DocType != null)
            entity.DocType = request.DocType;

        if (request.StoragePath != null)
            entity.StoragePath = request.StoragePath;

        if (request.Verified.HasValue)
            entity.Verified = request.Verified.Value;

        _unitOfWork.Documents.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
