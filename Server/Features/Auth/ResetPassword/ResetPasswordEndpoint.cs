using Carter;
using MediatR;
using Server.Helpers;
using Server.Utils.CustomResult;
using ResetPasswordRequest = Server.Contracts.ResetPasswordRequest;

namespace Server.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Paths.ResetPassword, async (ResetPasswordRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new ResetPasswordCommand(
                request.NewPassword, 
                request.ConfirmNewPassword, 
                request.Email, 
                request.ResetToken
            );
            var result =  await sender.Send(command, ct);
            
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
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity);
    }
}