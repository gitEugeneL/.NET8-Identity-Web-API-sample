using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;

namespace Server.Features;

public class EmailConfirmation : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/aut/email-confirmation", async (string email, string confirmationToken, ISender sender) =>
            {
                var command = new Command(email, confirmationToken);
                return await sender.Send(command);
            })
            .WithTags("Authentication");
    }

    public sealed record Command(
        string Email,
        string ConfirmationToken
    ) : IRequest<IResult>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(command => command.ConfirmationToken)
                .NotEmpty();
        }
    }

    internal sealed class Handler(
        UserManager<User> userManager
    ) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(command.Email);
            if (user is null)
                return Results.BadRequest();

            var confirmResult = await userManager.ConfirmEmailAsync(user, command.ConfirmationToken);

            return confirmResult.Succeeded
                ? Results.Ok()
                : Results.BadRequest();
        }
    }
}
