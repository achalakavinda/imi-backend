using MigratingAssistant.Application.Jobs.Commands.CreateJob;
using MigratingAssistant.Application.Jobs.Commands.UpdateJob;
using MigratingAssistant.Application.Jobs.Commands.DeleteJob;
using MigratingAssistant.Application.Jobs.Queries;
using MigratingAssistant.Application.Jobs.Queries.GetJobs;
using MigratingAssistant.Application.Jobs.Queries.GetJobById;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class Jobs : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetJobs);
        group.MapGet("/{id:guid}", GetJobById);
        group.MapPost("/", CreateJob);
        group.MapPut("/{id:guid}", UpdateJob);
        group.MapDelete("/{id:guid}", DeleteJob);
    }

    public async Task<List<JobDto>> GetJobs(ISender sender)
    {
        return await sender.Send(new GetJobsQuery());
    }

    public async Task<Results<Ok<JobDto>, NotFound>> GetJobById(ISender sender, Guid id)
    {
        try
        {
            var result = await sender.Send(new GetJobByIdQuery(id));
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateJob(ISender sender, CreateJobCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/jobs/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateJob(ISender sender, Guid id, UpdateJobCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteJob(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteJobCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
