namespace MigratingAssistant.Domain.Constants;

public abstract class Policies
{
    public const string CanPurge = nameof(CanPurge);
    public const string RequireAdministratorRole = nameof(RequireAdministratorRole);
    public const string RequireUserRole = nameof(RequireUserRole);
    public const string RequireGuestRole = nameof(RequireGuestRole);
}