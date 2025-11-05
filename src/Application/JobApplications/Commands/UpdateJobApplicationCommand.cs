using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.JobApplications.Commands;

public record UpdateJobApplicationCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid? JobId { get; init; }
    public Guid? ResumeFileId { get; init; }
    public JobApplicationSubmissionStatus? Status { get; init; }
}

public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateJobApplicationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.JobApplications.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(JobApplication), request.Id.ToString());
        }

        if (request.JobId.HasValue)
            entity.JobId = request.JobId.Value;

        if (request.ResumeFileId.HasValue)
            entity.ResumeFileId = request.ResumeFileId.Value;

        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        _unitOfWork.JobApplications.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
