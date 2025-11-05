namespace MigratingAssistant.Infrastructure.Data;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string Provider { get; set; } = "SqlServer";
    public string ConnectionString { get; set; } = string.Empty;
}

public static class DatabaseProvider
{
    public const string SqlServer = "SqlServer";
    public const string MySql = "MySql";
}