using Carter;
using MediatR;
using Server.Contracts;
using Server.Helpers;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Refresh;

public class RefreshEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Paths.Refresh, async (RefreshRequest request, ISender sender, CancellationToken ct) => 
            {
                var command = new RefreshCommand(request.RefreshToken);
                var result = await sender.Send(command, ct);

                if (result.IsFailure)
                    return result.Error switch
                    {
                        Errors.Validation => Results.UnprocessableEntity(result.Error.Body),
                        Errors.Authentication => Results.BadRequest(result.Error.Body),
                        _ => Results.Forbid()
                    };

                var response = new RefreshResponse(
                    result.Value.ConnectedDevices,
                    result.Value.AccessToken,
                    result.Value.RefreshToken,
                    result.Value.ExpireRefreshToken
                );

                return Results.Ok(response);
            })
            .WithTags("Authentication")
            .Produces<RefreshResponse>()
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status400BadRequest);
    }
}