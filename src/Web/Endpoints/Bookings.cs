using MigratingAssistant.Application.Bookings.Commands;
using MigratingAssistant.Application.Bookings.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class Bookings : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetBookings);
        group.MapGet("/{id:guid}", GetBooking);
        group.MapPost("/", CreateBooking);
        group.MapPut("/{id:guid}", UpdateBooking);
        group.MapDelete("/{id:guid}", DeleteBooking);
    }

    public async Task<List<BookingDto>> GetBookings(ISender sender)
    {
        return await sender.Send(new GetBookingsQuery());
    }

    public async Task<Results<Ok<BookingDto>, NotFound>> GetBooking(ISender sender, Guid id)
    {
        var booking = await sender.Send(new GetBookingByIdQuery(id));
        return booking is not null ? TypedResults.Ok(booking) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateBooking(ISender sender, CreateBookingCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/bookings/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateBooking(ISender sender, Guid id, UpdateBookingCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteBooking(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteBookingCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
