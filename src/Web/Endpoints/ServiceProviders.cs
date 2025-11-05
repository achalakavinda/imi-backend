using MigratingAssistant.Application.ServiceProviders.Commands;
using MigratingAssistant.Application.ServiceProviders.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class ServiceProviders : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetServiceProviders);
        group.MapGet("/{id:guid}", GetServiceProvider);
        group.MapPost("/", CreateServiceProvider);
        group.MapPut("/{id:guid}", UpdateServiceProvider);
        group.MapDelete("/{id:guid}", DeleteServiceProvider);
    }

    public async Task<List<ServiceProviderDto>> GetServiceProviders(ISender sender)
    {
        return await sender.Send(new GetServiceProvidersWithDetailsQuery());
    }

    public async Task<Results<Ok<ServiceProviderDto>, NotFound>> GetServiceProvider(ISender sender, Guid id)
    {
        var serviceProvider = await sender.Send(new GetServiceProviderByIdWithDetailsQuery(id));
        return serviceProvider is not null ? TypedResults.Ok(serviceProvider) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateServiceProvider(ISender sender, CreateServiceProviderCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/serviceproviders/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateServiceProvider(ISender sender, Guid id, UpdateServiceProviderCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteServiceProvider(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteServiceProviderCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
