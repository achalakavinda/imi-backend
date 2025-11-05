using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MigratingAssistant.Infrastructure.Data.Migrations.MySQL;

public static class MySqlMigrationConfiguration
{
    public static void ConfigureForMySql(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure auto-increment columns
            var properties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(int) || p.ClrType == typeof(long))
                .Where(p => p.ValueGenerated == ValueGenerated.OnAdd);

            foreach (var property in properties)
            {
                property.SetValueGenerationStrategy(MySqlValueGenerationStrategy.IdentityColumn);
            }
        }
    }
}