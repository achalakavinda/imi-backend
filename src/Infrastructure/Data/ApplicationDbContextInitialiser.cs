using MigratingAssistant.Domain.Constants;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MigratingAssistant.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Seed all roles
        await SeedRolesAsync();

        // Seed default admin user
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { Roles.Administrator, Roles.User, Roles.Guest };

        foreach (var roleName in roles)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                _logger.LogInformation("Creating role: {RoleName}", roleName);
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        // Create default administrator account
        var adminEmail = "admin@migratingassistant.com";
        var administrator = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            _logger.LogInformation("Creating default administrator user: {Email}", adminEmail);

            var result = await _userManager.CreateAsync(administrator, "Admin@123456");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(administrator, Roles.Administrator);
                _logger.LogInformation("Administrator user created successfully");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create administrator user: {Errors}", errors);
            }
        }
    }
}
