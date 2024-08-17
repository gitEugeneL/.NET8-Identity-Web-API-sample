using Carter;
using MediatR;
using Server.Helpers;
using Server.Utils.CustomResult;
using ForgotPasswordRequest = Server.Contracts.ForgotPasswordRequest;

namespace Server.Features.Auth.ForgotPassword;

public class ForgotPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Paths.ForgotPassword, async (ForgotPasswordRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new ForgotPasswordCommand(request.Email, request.ClientUri);
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
        .WithTags("Authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status400BadRequest);
    }
}