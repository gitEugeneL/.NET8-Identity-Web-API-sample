using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Server.Contracts;
using Server.Domain.Entities;
using Server.Helpers;
using Server.Services.Interfaces;

namespace Server.Features;

public class Registration : ICarterModule
{ 
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/registration", async (RegistrationRequest request, ISender sender) =>
            {
                var command = new Command(
                    Email: request.Email,
                    Password: request.Password,
                    ConfirmPassword: request.Password,
                    Username: request.Username,
                    ClientUri: request.ClientUri,
                    FirstName: request.FirstName,
                    LastName: request.LastName,
                    Age: request.Age
                );
                return await sender.Send(command);
            })
            .WithTags("Authentication")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status400BadRequest);
    }
    
    public sealed record Command(
        string Email,
        string Password,
        string ConfirmPassword,
        string Username,
        string ClientUri,
        string? FirstName = null,
        string? LastName = null,
        int? Age = null    
    ) : IRequest<IResult>;
    
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(command => command.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(20)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$")
                .WithMessage("The password must contain at least one letter, one special character, and one digit");
            
            RuleFor(command => command.ConfirmPassword)
                .NotEmpty()
                .Equal(command => command.Password)
                .WithMessage("Passwords do not match");

            RuleFor(command => command.Username)
                .NotEmpty()
                .Matches( "^[a-zA-Z0-9-_]*$")
                .WithMessage("Username must not contain special characters.");

            RuleFor(company => company.ClientUri)
                .NotEmpty();
            
            RuleFor(command => command.FirstName)
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(command => command.LastName)
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(command => command.Age)
                .LessThanOrEqualTo(120)
                .GreaterThanOrEqualTo(18);
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
            
            var user = new User
            {
                UserName = command.Username,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Age = command.Age,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(user, command.Password);

            if (!createResult.Succeeded)
                return Results.BadRequest(createResult.Errors.Select(e => e.Description));

            await userManager.AddToRoleAsync(user, AppConstants.UserRole);
            
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var param = new Dictionary<string, string>
            {
                { "confirmationToken", confirmationToken },
                { "email", user.Email }
            };
            var callback = QueryHelpers.AddQueryString(command.ClientUri, param);
            
            await mailService.SendMailAsync(user.Email, "Email Confirmation token", callback);

            return Results.Created();
        }
    }
}