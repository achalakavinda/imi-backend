using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.JobApplications.Commands;

public record DeleteJobApplicationCommand(Guid Id) : IRequest;

public class DeleteJobApplicationCommandHandler : IRequestHandler<DeleteJobApplicationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJobApplicationCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.JobApplications.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(JobApplication), request.Id.ToString());
        }

        _unitOfWork.JobApplications.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
