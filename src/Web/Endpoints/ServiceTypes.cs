using MigratingAssistant.Application.ServiceTypes.Commands.CreateServiceType;
using MigratingAssistant.Application.ServiceTypes.Commands.UpdateServiceType;
using MigratingAssistant.Application.ServiceTypes.Commands.DeleteServiceType;
using MigratingAssistant.Application.ServiceTypes.Queries;
using MigratingAssistant.Application.ServiceTypes.Queries.GetServiceTypes;
using MigratingAssistant.Application.ServiceTypes.Queries.GetServiceTypeById;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class ServiceTypes : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetServiceTypes);
        group.MapGet("/{id:int}", GetServiceTypeById);
        group.MapPost("/", CreateServiceType);
        group.MapPut("/{id:int}", UpdateServiceType);
        group.MapDelete("/{id:int}", DeleteServiceType);
    }

    public async Task<List<ServiceTypeDto>> GetServiceTypes(ISender sender)
    {
        return await sender.Send(new GetServiceTypesQuery());
    }

    public async Task<Results<Ok<ServiceTypeDto>, NotFound>> GetServiceTypeById(ISender sender, int id)
    {
        try
        {
            var result = await sender.Send(new GetServiceTypeByIdQuery(id));
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Results<Created<int>, BadRequest<string>>> CreateServiceType(ISender sender, CreateServiceTypeCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/servicetypes/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateServiceType(ISender sender, int id, UpdateServiceTypeCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteServiceType(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteServiceTypeCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
