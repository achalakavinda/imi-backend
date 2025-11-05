using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Domain.Constants;
using MigratingAssistant.Infrastructure.Authentication;
using MigratingAssistant.Infrastructure.Data;
using MigratingAssistant.Infrastructure.Data.Interceptors;
using MigratingAssistant.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        // Configure database options
        var dbOptions = new DatabaseOptions();
        builder.Configuration.GetSection(DatabaseOptions.SectionName).Bind(dbOptions);
        Guard.Against.NullOrWhiteSpace(dbOptions.ConnectionString, message: "Database connection string not found.");
        Guard.Against.NullOrWhiteSpace(dbOptions.Provider, message: "Database provider not specified.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            switch (dbOptions.Provider.ToLowerInvariant())
            {
                case "mysql":
                    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
                    options.UseMySql(dbOptions.ConnectionString, serverVersion);
                    break;

                case "sqlserver":
                default:
                    options.UseSqlServer(dbOptions.ConnectionString);
                    break;
            }

            options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        // Configure JWT settings
        var jwtSettings = new JwtSettings();
        builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

        Guard.Against.NullOrWhiteSpace(jwtSettings.Secret, message: "JWT Secret not found in configuration.");
        Guard.Against.NullOrWhiteSpace(jwtSettings.Issuer, message: "JWT Issuer not found in configuration.");
        Guard.Against.NullOrWhiteSpace(jwtSettings.Audience, message: "JWT Audience not found in configuration.");

        // Configure JWT Bearer authentication
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
                };
            });

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthorization(options =>
        {
            // Legacy policy
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));

            // Role-based policies
            options.AddPolicy(Policies.RequireAdministratorRole, policy =>
                policy.RequireRole(Roles.Administrator));

            options.AddPolicy(Policies.RequireUserRole, policy =>
                policy.RequireRole(Roles.User, Roles.Administrator)); // Admin can do everything User can

            options.AddPolicy(Policies.RequireGuestRole, policy =>
                policy.RequireRole(Roles.Guest, Roles.User, Roles.Administrator)); // Any authenticated user
        });
    }
}
