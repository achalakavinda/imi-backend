using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.JobApplications.Commands;

public record CreateJobApplicationCommand : IRequest<Guid>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public Guid? ResumeFileId { get; init; }
    public JobApplicationSubmissionStatus Status { get; init; }
}

public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobApplicationCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        var job = await _unitOfWork.Jobs.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            throw new NotFoundException(nameof(Job), request.JobId.ToString());
        }

        // Validate resume file if provided
        if (request.ResumeFileId.HasValue)
        {
            var resumeFile = await _unitOfWork.Documents.GetByIdAsync(request.ResumeFileId.Value, cancellationToken);
            if (resumeFile == null)
            {
                throw new NotFoundException(nameof(Document), request.ResumeFileId.Value.ToString());
            }
        }

        var entity = new JobApplication
        {
            JobId = request.JobId,
            UserId = request.UserId,
            ResumeFileId = request.ResumeFileId,
            Status = request.Status,
            AppliedAt = DateTimeOffset.UtcNow
        };

        _unitOfWork.JobApplications.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}