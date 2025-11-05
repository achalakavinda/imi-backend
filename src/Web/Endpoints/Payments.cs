using MigratingAssistant.Application.Payments.Commands;
using MigratingAssistant.Application.Payments.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class Payments : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetPayments);
        group.MapGet("/{id:guid}", GetPayment);
        group.MapPost("/", CreatePayment);
        group.MapPut("/{id:guid}", UpdatePayment);
        group.MapDelete("/{id:guid}", DeletePayment);
    }

    public async Task<List<PaymentDto>> GetPayments(ISender sender)
    {
        return await sender.Send(new GetPaymentsQuery());
    }

    public async Task<Results<Ok<PaymentDto>, NotFound>> GetPayment(ISender sender, Guid id)
    {
        var payment = await sender.Send(new GetPaymentByIdQuery(id));
        return payment is not null ? TypedResults.Ok(payment) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreatePayment(ISender sender, CreatePaymentCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/payments/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdatePayment(ISender sender, Guid id, UpdatePaymentCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeletePayment(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeletePaymentCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
