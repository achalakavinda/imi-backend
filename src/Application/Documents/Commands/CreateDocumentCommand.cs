using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Documents.Commands;

public record CreateDocumentCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string DocType { get; init; } = string.Empty;
    public string StoragePath { get; init; } = string.Empty;
    public bool Verified { get; init; }
}

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDocumentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists using repository
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        // Create document entity
        var entity = new Document
        {
            UserId = request.UserId,
            DocType = request.DocType,
            StoragePath = request.StoragePath,
            Verified = request.Verified,
            UploadedAt = DateTimeOffset.UtcNow
        };

        // Insert using repository
        _unitOfWork.Documents.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
