using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.UserProfiles.Commands;

public record UpdateUserProfileCommand : IRequest
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public string? Nationality { get; init; }
    public string? Bio { get; init; }
    public string? Preferences { get; init; }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.UserProfiles.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(UserProfile), request.Id.ToString());
        }

        if (request.FirstName != null)
            entity.FirstName = request.FirstName;

        if (request.LastName != null)
            entity.LastName = request.LastName;

        if (request.Phone != null)
            entity.Phone = request.Phone;

        if (request.Nationality != null)
            entity.Nationality = request.Nationality;

        if (request.Bio != null)
            entity.Bio = request.Bio;

        if (request.Preferences != null)
            entity.Preferences = request.Preferences;

        _unitOfWork.UserProfiles.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
