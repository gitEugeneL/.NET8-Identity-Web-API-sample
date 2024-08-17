using Carter;
using MediatR;
using Server.Helpers;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.EmailConfirmation;

public class ConfirmationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Paths.EmailConfirmation, 
                async (string email, string confirmationToken, ISender sender, CancellationToken ct) =>
            {
                var command = new ConfirmationCommand(email, confirmationToken);
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
            .Produces(StatusCodes.Status400BadRequest);
    }
}