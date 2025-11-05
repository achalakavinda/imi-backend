using MigratingAssistant.Application.Users.Commands.CreateUser;
using MigratingAssistant.Application.Users.Queries.GetUsers;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Infrastructure.Identity;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapIdentityApi<ApplicationUser>();
        
        // Custom CQRS endpoints
        group.MapGet("/list", GetUsers).RequireAuthorization();
        group.MapPost("/register", CreateUser);  // No authorization required for registration
    }

    public async Task<Ok<IList<UserBriefDto>>> GetUsers(ISender sender)
    {
        var users = await sender.Send(new GetUsersQuery());
        return TypedResults.Ok(users);
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateUser(ISender sender, CreateUserCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/users/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
