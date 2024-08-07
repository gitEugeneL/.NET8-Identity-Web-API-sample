using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Server.Contracts;
using Server.Domain.Entities;

namespace Server.Features;

public class ResetPassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/reset-password", async (ResetPasswordRequest request, ISender sender) =>
            {
                var command = new Command(
                    request.NewPassword, 
                    request.ConfirmNewPassword, 
                    request.Email, 
                    request.ResetToken
                );
                return await sender.Send(command);
            })
            .WithTags("Authentication");
    }

    public sealed record Command(
        string NewPassword,
        string ConfirmNewPassword,
        string Email,
        string ResetToken
    ) : IRequest<IResult>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(20)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$")
                .WithMessage("The password must contain at least one letter, one special character, and one digit");

            
            RuleFor(command => command.ConfirmNewPassword)
                .NotEmpty()
                .Equal(command => command.NewPassword)
                .WithMessage("Passwords do not match");

            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(command => command.ResetToken)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(
        IValidator<Command> validator,
        UserManager<User> userManager
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

            var resetResult = await userManager.ResetPasswordAsync(user, command.ResetToken, command.NewPassword);

            if (resetResult.Succeeded) 
                return Results.Ok();
            
            var errors = resetResult.Errors.Select(e => e.Description);
            return Results.BadRequest(new { Errors = errors });
        }
    }
}