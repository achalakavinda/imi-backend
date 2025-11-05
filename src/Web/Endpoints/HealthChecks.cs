namespace MigratingAssistant.Web.Endpoints;

public class HealthChecks : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        }))
        .WithName("HealthCheck")
        .WithOpenApi()
        .AllowAnonymous();

        group.MapGet("/health/ready", () => Results.Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow
        }))
        .WithName("ReadinessCheck")
        .WithOpenApi()
        .AllowAnonymous();
    }
}
