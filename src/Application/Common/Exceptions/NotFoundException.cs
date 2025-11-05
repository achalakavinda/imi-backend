namespace MigratingAssistant.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, string key)
        : base($"{name} ({key}) was not found.")
    {
    }
}
