using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Server.Contracts;
using Server.Domain.Entities;
using Server.Services.Interfaces;

namespace Server.Features;

public class ForgotPassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/forgot-password", async (ForgotPasswordRequest request, ISender sender) =>
            {
                var command = new Command(request.Email, request.ClientUri);
                return await sender.Send(command);
            })
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public sealed record Command(
        string Email,
        string ClientUri
    ) : IRequest<IResult>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(command => command.ClientUri)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(
        IValidator<Command> validator,
        UserManager<User> userManager,
        IMailService mailService
    ) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return Results.UnprocessableEntity(validationResult.GetValidationProblems());
            
            var user = await userManager.FindByEmailAsync(command.Email);
            if (user is null)
                return Results.BadRequest();
            
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string>
            {
                { "token", resetToken },
                { "email", command.Email }
            };
            var callback = QueryHelpers.AddQueryString(command.ClientUri, param); 
            
            return await mailService.SendMailAsync(user.Email, "Reset password token", callback)
                ? Results.Ok()
                : Results.BadRequest();
        }
    }
}