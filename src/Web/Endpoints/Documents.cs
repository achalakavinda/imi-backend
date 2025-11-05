using MigratingAssistant.Application.Documents.Commands;
using MigratingAssistant.Application.Documents.Queries;
using ValidationException = MigratingAssistant.Application.Common.Exceptions.ValidationException;
using NotFoundException = MigratingAssistant.Application.Common.Exceptions.NotFoundException;
using MigratingAssistant.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MigratingAssistant.Web.Endpoints;

public class Documents : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetDocuments);
        group.MapGet("/{id:guid}", GetDocument);
        group.MapPost("/", CreateDocument);
        group.MapPut("/{id:guid}", UpdateDocument);
        group.MapDelete("/{id:guid}", DeleteDocument);
    }

    public async Task<List<DocumentDto>> GetDocuments(ISender sender)
    {
        return await sender.Send(new GetDocumentsQuery());
    }

    public async Task<Results<Ok<DocumentDto>, NotFound>> GetDocument(ISender sender, Guid id)
    {
        var document = await sender.Send(new GetDocumentByIdQuery(id));
        return document is not null ? TypedResults.Ok(document) : TypedResults.NotFound();
    }

    public async Task<Results<Created<Guid>, BadRequest<string>>> CreateDocument(ISender sender, CreateDocumentCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/documents/{id}", id);
        }
        catch (ValidationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> UpdateDocument(ISender sender, Guid id, UpdateDocumentCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteDocument(ISender sender, Guid id)
    {
        try
        {
            await sender.Send(new DeleteDocumentCommand(id));
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
