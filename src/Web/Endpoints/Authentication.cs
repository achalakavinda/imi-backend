using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Infrastructure.Authentication;
using MigratingAssistant.Infrastructure.Data;
using MigratingAssistant.Infrastructure.Identity;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Constants;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MigratingAssistant.Web.Endpoints;

public class Authentication : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost("/login", Login).AllowAnonymous();
        group.MapPost("/register", Register).AllowAnonymous();
        group.MapPost("/register-guest", RegisterGuest).AllowAnonymous();
        group.MapPost("/refresh", RefreshToken).AllowAnonymous();
        group.MapPost("/revoke", RevokeToken).RequireAuthorization();
    }

    public async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> Login(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return TypedResults.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1) // From JwtSettings.RefreshTokenExpirationInDays
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 60 minutes in seconds
            TokenType = "Bearer"
        });
    }

    public async Task<Results<Ok<AuthResponse>, BadRequest<string>>> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext)
    {
        return await RegisterUserWithRole(request, userManager, jwtTokenService, dbContext, Roles.User);
    }

    public async Task<Results<Ok<AuthResponse>, BadRequest<string>>> RegisterGuest(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext)
    {
        return await RegisterUserWithRole(request, userManager, jwtTokenService, dbContext, Roles.Guest);
    }

    private async Task<Results<Ok<AuthResponse>, BadRequest<string>>> RegisterUserWithRole(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return TypedResults.BadRequest("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return TypedResults.BadRequest(errors);
        }

        // Assign role to user
        await userManager.AddToRoleAsync(user, role);

        // Create corresponding User entity for business logic
        var userRole = role switch
        {
            Roles.Administrator => Domain.Enums.UserRole.Admin,
            Roles.Guest => Domain.Enums.UserRole.User, // Guest maps to User in the domain
            Roles.User => Domain.Enums.UserRole.User,
            _ => Domain.Enums.UserRole.User
        };

        var userEntity = new Domain.Entities.User
        {
            Id = Guid.Parse(user.Id), // Use the same ID as the ApplicationUser
            Email = user.Email,
            PasswordHash = null, // Password is managed by Identity, not stored in User entity
            Role = userRole,
            EmailVerified = false
        };

        dbContext.Users.Add(userEntity);

        var roles = new List<string> { role };
        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }

    public async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> RefreshToken(
        RefreshTokenRequest request,
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        // Validate the expired access token and extract claims
        var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return TypedResults.Unauthorized();
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.Unauthorized();
        }

        // Validate refresh token from database
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

        if (storedRefreshToken == null || !storedRefreshToken.IsActive)
        {
            return TypedResults.Unauthorized();
        }

        // Get user and generate new tokens
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return TypedResults.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var newAccessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        // Revoke old refresh token and create new one (token rotation)
        storedRefreshToken.RevokedAt = DateTime.UtcNow;

        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }

    public async Task<Results<Ok, UnauthorizedHttpResult>> RevokeToken(
        RevokeTokenRequest request,
        ICurrentUserService currentUserService,
        ApplicationDbContext dbContext,
        HttpContext httpContext)
    {
        var userId = currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.Unauthorized();
        }

        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

        if (refreshToken == null)
        {
            return TypedResults.Unauthorized();
        }

        // Revoke the refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Also blacklist the current access token
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var accessToken = authHeader.Substring("Bearer ".Length).Trim();

            var revokedToken = new RevokedToken
            {
                Token = accessToken,
                UserId = userId,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Match access token expiry
                Reason = "User requested token revocation"
            };

            dbContext.RevokedTokens.Add(revokedToken);
        }

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);

public record AuthResponse
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; } = null!;
}
