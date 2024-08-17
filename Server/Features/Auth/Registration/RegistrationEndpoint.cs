using Carter;
using MediatR;
using Server.Contracts;
using Server.Helpers;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Registration;

public class RegistrationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Paths.Registration, async (RegistrationRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new RegistrationCommand(
                    Email: request.Email,
                    Password: request.Password,
                    ConfirmPassword: request.ConfirmPassword,
                    Username: request.Username,
                    ClientUri: request.ClientUri,
                    FirstName: request.FirstName,
                    LastName: request.LastName,
                    Age: request.Age
                );

                var result = await sender.Send(command, ct);

                if (result.IsFailure)
                    return result.Error switch
                    {
                        Errors.Validation => Results.UnprocessableEntity(result.Error.Body),
                        Errors.Authentication => Results.BadRequest(result.Error.Body),
                        _ => Results.Forbid()
                    };

                return Results.Created(Paths.Registration, result.Value.UserId);
            })
            .WithTags("Authentication")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status400BadRequest);
    }
}