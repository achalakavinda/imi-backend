using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MigratingAssistant.Domain.Enums;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;

namespace MigratingAssistant.Application.Users.Commands;

public record UpdateUserCommand : IRequest
{
    public Guid Id { get; init; }
    public string? Email { get; init; }
    public UserRole? Role { get; init; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(User), request.Id.ToString());
        }

        entity.Email = request.Email;
        if (request.Role.HasValue)
        {
            entity.Role = request.Role.Value;
        }

        _unitOfWork.Users.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
