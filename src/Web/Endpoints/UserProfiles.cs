using MigratingAssistant.Application.UserProfiles.Commands;
using MigratingAssistant.Application.UserProfiles.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class UserProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetUserProfiles);
        group.MapGet("/{id:guid}", GetUserProfile);
        group.MapPost("/", CreateUserProfile);
        group.MapPut("/{id:guid}", UpdateUserProfile);
        group.MapDelete("/{id:guid}", DeleteUserProfile);
    }

    public async Task<List<UserProfileDto>> GetUserProfiles(ISender sender)
    {
        return await sender.Send(new GetUserProfilesQuery());
    }

    public async Task<Results<Ok<UserProfileDto>, NotFound>> GetUserProfile(ISender sender, Guid id)
    {
        var userProfile = await sender.Send(new GetUserProfileByIdQuery(id));
        return userProfile is not null ? TypedResults.Ok(userProfile) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateUserProfile(ISender sender, CreateUserProfileCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/userprofiles/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateUserProfile(ISender sender, Guid id, UpdateUserProfileCommand command)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest("ID mismatch");
        }

        try
        {
            await sender.Send(command);
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound>> DeleteUserProfile(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteUserProfileCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
