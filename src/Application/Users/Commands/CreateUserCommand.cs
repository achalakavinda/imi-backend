using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Application.Users.Commands;

public record CreateUserCommand : IRequest<Guid>
{
    public string? Email { get; init; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User
        {
            Email = request.Email
        };

        _unitOfWork.Users.Insert(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
