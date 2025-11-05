using MigratingAssistant.Application.Listings.Commands.CreateListing;
using MigratingAssistant.Application.Listings.Commands.UpdateListing;
using MigratingAssistant.Application.Listings.Commands.DeleteListing;
using MigratingAssistant.Application.Listings.Queries;
using MigratingAssistant.Application.Listings.Queries.GetListings;
using MigratingAssistant.Application.Listings.Queries.GetListingById;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class Listings : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetListings);
        group.MapGet("/{id:guid}", GetListingById);
        group.MapPost("/", CreateListing);
        group.MapPut("/{id:guid}", UpdateListing);
        group.MapDelete("/{id:guid}", DeleteListing);
    }

    public async Task<List<ListingDto>> GetListings(ISender sender)
    {
        return await sender.Send(new GetListingsQuery());
    }

    public async Task<Results<Ok<ListingDto>, NotFound>> GetListingById(ISender sender, Guid id)
    {
        try
        {
            var result = await sender.Send(new GetListingByIdQuery(id));
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateListing(ISender sender, CreateListingCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/listings/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateListing(ISender sender, Guid id, UpdateListingCommand command)
    {
        if (id != command.Id)
        {
            return TypedResults.BadRequest("Id mismatch");
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

    public async Task<Results<NoContent, NotFound>> DeleteListing(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteListingCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
