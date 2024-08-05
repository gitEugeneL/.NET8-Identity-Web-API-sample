using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Server.Contracts;
using Server.Domain.Entities;

namespace Server.Features;

public class Registration : ICarterModule
{ 
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/registration", async (RegistrationRequest request, ISender sender) =>
            {
                var command = new Commnad(
                    Email: request.Email,
                    Password: request.Password,
                    ConfirmPassword: request.Password,
                    Username: request.Username,
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
    
    public sealed record Commnad(
        string Email,
        string Password,
        string ConfirmPassword,
        string Username,
        string? FirstName = null,
        string? LastName = null,
        int? Age = null    
    ) : IRequest<IResult>;
    
    
    public sealed class Validator : AbstractValidator<Commnad>
    {
        public Validator()
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(request => request.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(20)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$")
                .WithMessage("The password must contain at least one letter, one special character, and one digit");
            
            RuleFor(request => request.ConfirmPassword)
                .NotEmpty()
                .Equal(c => c.Password)
                .WithMessage("Passwords do not match");

            RuleFor(request => request.Username)
                .NotEmpty()
                .Matches( "^[a-zA-Z0-9-_]*$")
                .WithMessage("Username must not contain special characters.");
            
            RuleFor(request => request.FirstName)
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(request => request.LastName)
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(request => request.Age)
                .LessThanOrEqualTo(120)
                .GreaterThanOrEqualTo(18);
        }
    }
    
    internal sealed class Handler(
        IValidator<Commnad> validator,
        UserManager<User> userManager
    ) : IRequestHandler<Commnad, IResult>
    {
        public async Task<IResult> Handle(Commnad commnad, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(commnad, cancellationToken);
            if (!validationResult.IsValid)
                return Results.UnprocessableEntity(validationResult.GetValidationProblems());
            
            var user = new User
            {
                UserName = commnad.Username,
                Email = commnad.Email,
                FirstName = commnad.FirstName,
                LastName = commnad.LastName,
                Age = commnad.Age,
                CreatedAt = DateTime.UtcNow
            };
        
            var createResult = await userManager.CreateAsync(user, commnad.Password);

            return createResult.Succeeded 
                ? Results.Created() 
                : Results.BadRequest(createResult.Errors.Select(e => e.Description));
        }
    }
}