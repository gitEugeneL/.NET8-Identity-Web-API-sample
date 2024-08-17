using Carter;
using MediatR;
using Server.Contracts;
using Server.Helpers;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Login;

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Paths.Login, async (LoginRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new LoginCommand(request.Email, request.Password);
                var result = await sender.Send(command, ct);
                
                if (result.IsFailure)
                    return result.Error switch
                    {
                        Errors.Validation => Results.UnprocessableEntity(result.Error.Body),
                        Errors.Authentication => Results.BadRequest(result.Error.Body),
                        _ => Results.Forbid()
                    };
                
                var response = new LoginResponse(
                    result.Value.AccessToken, 
                    result.Value.RefreshToken, 
                    result.Value.ExpireRefreshToken
                );
                
                return Results.Ok(response);
            })
            .WithTags("Authentication")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }
}