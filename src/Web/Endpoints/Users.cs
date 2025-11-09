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
        // The project previously called MapIdentityApi which registers default
        // identity endpoints (including a /register route). That produced an
        // AmbiguousMatchException because this class also declared a
        // custom CQRS registration endpoint at "/register".
        //
        // To avoid route collisions we do not auto-register the identity API
        // here and instead expose only the CQRS endpoints below.

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
