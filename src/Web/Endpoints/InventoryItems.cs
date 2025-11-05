using MigratingAssistant.Application.InventoryItems.Commands.CreateInventoryItem;
using MigratingAssistant.Application.InventoryItems.Commands.UpdateInventoryItem;
using MigratingAssistant.Application.InventoryItems.Commands.DeleteInventoryItem;
using MigratingAssistant.Application.InventoryItems.Queries;
using MigratingAssistant.Application.InventoryItems.Queries.GetInventoryItems;
using MigratingAssistant.Application.InventoryItems.Queries.GetInventoryItemById;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class InventoryItems : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetInventoryItems);
        group.MapGet("/{id:guid}", GetInventoryItemById);
        group.MapPost("/", CreateInventoryItem);
        group.MapPut("/{id:guid}", UpdateInventoryItem);
        group.MapDelete("/{id:guid}", DeleteInventoryItem);
    }

    public async Task<List<InventoryItemDto>> GetInventoryItems(ISender sender)
    {
        return await sender.Send(new GetInventoryItemsQuery());
    }

    public async Task<Results<Ok<InventoryItemDto>, NotFound>> GetInventoryItemById(ISender sender, Guid id)
    {
        try
        {
            var result = await sender.Send(new GetInventoryItemByIdQuery(id));
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateInventoryItem(ISender sender, CreateInventoryItemCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/inventoryitems/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateInventoryItem(ISender sender, Guid id, UpdateInventoryItemCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteInventoryItem(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteInventoryItemCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
