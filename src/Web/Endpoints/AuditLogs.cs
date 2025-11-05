using MigratingAssistant.Application.AuditLogs.Queries;
using MigratingAssistant.Application.AuditLogs.Queries.GetAuditLogs;
using MigratingAssistant.Application.AuditLogs.Queries.GetAuditLogById;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class AuditLogs : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAuditLogs);
        group.MapGet("/{id:long}", GetAuditLogById);
    }

    public async Task<List<AuditLogDto>> GetAuditLogs(ISender sender)
    {
        return await sender.Send(new GetAuditLogsQuery());
    }

    public async Task<Results<Ok<AuditLogDto>, NotFound>> GetAuditLogById(ISender sender, long id)
    {
        try
        {
            var result = await sender.Send(new GetAuditLogByIdQuery(id));
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
