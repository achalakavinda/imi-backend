using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigratingAssistant.Infrastructure.Data;

namespace MigratingAssistant.Infrastructure.Authentication;

/// <summary>
/// Custom JWT Bearer handler that checks if the token has been revoked.
/// This prevents the use of valid but revoked access tokens.
/// </summary>
public class CustomJwtBearerHandler : JwtBearerHandler
{
    private readonly ApplicationDbContext _dbContext;

    public CustomJwtBearerHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ApplicationDbContext dbContext)
        : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // First, let the base handler validate the token (signature, expiry, etc.)
        var result = await base.HandleAuthenticateAsync();

        if (!result.Succeeded)
        {
            return result;
        }

        // Extract the token from the Authorization header
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return AuthenticateResult.Fail("Invalid Authorization header");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Check if the token has been revoked
        var isRevoked = await _dbContext.RevokedTokens
            .AnyAsync(rt => rt.Token == token && rt.ExpiresAt > DateTime.UtcNow);

        if (isRevoked)
        {
            return AuthenticateResult.Fail("Token has been revoked");
        }

        return result;
    }
}
