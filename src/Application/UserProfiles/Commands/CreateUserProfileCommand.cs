using MigratingAssistant.Domain.Entities;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.UserProfiles.Commands;

public record CreateUserProfileCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public string? Nationality { get; init; }
    public string? Bio { get; init; }
    public string? Preferences { get; init; } // JSON
}

public class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserProfileCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId.ToString());
        }

        var entity = new UserProfile
        {
            UserId = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Nationality = request.Nationality,
            Bio = request.Bio,
            Preferences = request.Preferences
        };

        _unitOfWork.UserProfiles.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}