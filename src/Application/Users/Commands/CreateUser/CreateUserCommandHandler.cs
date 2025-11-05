using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Events;

namespace MigratingAssistant.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // You'll need to add BCrypt.Net-Next NuGet package
            Role = request.Role,
            EmailVerified = false
        };

        _context.Users.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}