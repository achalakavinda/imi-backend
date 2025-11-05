using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Domain.Events;

public class UserCreatedEvent : BaseEvent
{
    public UserCreatedEvent(User user)
    {
        User = user;
    }

    public User User { get; }
}