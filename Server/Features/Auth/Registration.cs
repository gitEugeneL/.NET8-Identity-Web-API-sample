using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Server.Contracts.Auth;
using Server.Domain.Entities;

namespace Server.Features.Auth;

public class Registration : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/registration", async (RegistrationRequest request, ISender sender) =>
            {
                var command = new Command(
                    Email: request.Email,
                    Password: request.Password,
                    ConfirmPassword: request.ConfirmPassword,
                    FirstName: request.FirstName,
                    LastName: request.LastName,
                    Age: request.Age
                );
                return await sender.Send(command);
            })
            .WithTags("Authentication")
            .Produces<RegistrationResponse>(StatusCodes.Status201Created)
            .Produces<RegistrationResponse>(StatusCodes.Status400BadRequest);
    }
    public sealed record Command(
        string Email,
        string Password,
        string ConfirmPassword,
        string? FirstName,
        string? LastName,
        int? Age    
    ): IRequest<Results<Created<RegistrationResponse>, BadRequest<RegistrationResponse>, ValidationProblem>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(c => c.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(20)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$")
                .WithMessage("The password must contain at least one letter, one special character, and one digit");
            
            RuleFor(c => c.ConfirmPassword)
                .NotEmpty()
                .Equal(c => c.Password)
                .WithMessage("Passwords do not match");

            RuleFor(c => c.FirstName)
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(c => c.LastName)
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(c => c.Age)
                .LessThanOrEqualTo(120)
                .GreaterThanOrEqualTo(18);
        }
    }
    
    internal sealed class Handler(
        IValidator<Command> validator,
        UserManager<CustomIdentityUser> userManager) 
        : IRequestHandler<Command, Results<Created<RegistrationResponse>, BadRequest<RegistrationResponse>, ValidationProblem>>
    {
        public async Task<Results<Created<RegistrationResponse>, BadRequest<RegistrationResponse>, ValidationProblem>> Handle(
            Command command, 
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());
            
            var user = new CustomIdentityUser
            {
                UserName = command.Email,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Age = command.Age
            };

            var createResult = await userManager.CreateAsync(user, command.Password);
            if (createResult.Succeeded) 
                return TypedResults.Created(user.Id, new RegistrationResponse(true, []));
            
            var errors = createResult.Errors.Select(e => e.Description);
            return TypedResults.BadRequest(new RegistrationResponse(false, errors));
        }
    }
}