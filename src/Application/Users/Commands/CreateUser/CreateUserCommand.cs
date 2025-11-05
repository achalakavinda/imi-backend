using MigratingAssistant.Domain.Enums;
using MigratingAssistant.Domain.Events;

namespace MigratingAssistant.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Guid>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.User;
}