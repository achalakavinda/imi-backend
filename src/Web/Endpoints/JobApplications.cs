using MigratingAssistant.Application.JobApplications.Commands;
using MigratingAssistant.Application.JobApplications.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class JobApplications : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetJobApplications);
        group.MapGet("/{id:guid}", GetJobApplication);
        group.MapPost("/", CreateJobApplication);
        group.MapPut("/{id:guid}", UpdateJobApplication);
        group.MapDelete("/{id:guid}", DeleteJobApplication);
    }

    public async Task<List<JobApplicationDto>> GetJobApplications(ISender sender)
    {
        return await sender.Send(new GetJobApplicationsQuery());
    }

    public async Task<Results<Ok<JobApplicationDto>, NotFound>> GetJobApplication(ISender sender, Guid id)
    {
        var jobApplication = await sender.Send(new GetJobApplicationByIdQuery(id));
        return jobApplication is not null ? TypedResults.Ok(jobApplication) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateJobApplication(ISender sender, CreateJobApplicationCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/jobapplications/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateJobApplication(ISender sender, Guid id, UpdateJobApplicationCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteJobApplication(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteJobApplicationCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
