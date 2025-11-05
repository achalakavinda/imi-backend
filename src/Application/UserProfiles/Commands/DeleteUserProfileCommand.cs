using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.UserProfiles.Commands;

public record DeleteUserProfileCommand(Guid Id) : IRequest;

public class DeleteUserProfileCommandHandler : IRequestHandler<DeleteUserProfileCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserProfileCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.UserProfiles.GetByIdAsync(request.Id.ToString(), cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(UserProfile), request.Id.ToString());
        }

        _unitOfWork.UserProfiles.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
