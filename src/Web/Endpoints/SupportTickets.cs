using MigratingAssistant.Application.SupportTickets.Commands;
using MigratingAssistant.Application.SupportTickets.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class SupportTickets : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetSupportTickets);
        group.MapGet("/{id:guid}", GetSupportTicket);
        group.MapPost("/", CreateSupportTicket);
        group.MapPut("/{id:guid}", UpdateSupportTicket);
        group.MapDelete("/{id:guid}", DeleteSupportTicket);
    }

    public async Task<List<SupportTicketDto>> GetSupportTickets(ISender sender)
    {
        return await sender.Send(new GetSupportTicketsQuery());
    }

    public async Task<Results<Ok<SupportTicketDto>, NotFound>> GetSupportTicket(ISender sender, Guid id)
    {
        var supportTicket = await sender.Send(new GetSupportTicketByIdQuery(id));
        return supportTicket is not null ? TypedResults.Ok(supportTicket) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateSupportTicket(ISender sender, CreateSupportTicketCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/supporttickets/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateSupportTicket(ISender sender, Guid id, UpdateSupportTicketCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteSupportTicket(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteSupportTicketCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
