using Carter;
using MediatR;
using Server.Contracts;
using Server.Helpers;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Logout;

public class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Paths.Logout, async (RefreshRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new LogoutCommand(request.RefreshToken);
                var result = await sender.Send(command, ct);
                
                if (result.IsFailure)
                    return result.Error switch
                    {
                        Errors.Validation => Results.UnprocessableEntity(result.Error.Body),
                        Errors.Authentication => Results.BadRequest(result.Error.Body),
                        _ => Results.Forbid()
                    };
                
                return Results.Ok(result.Value);
            })
            .RequireAuthorization(Roles.User)
            
            .WithTags("Authentication")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}