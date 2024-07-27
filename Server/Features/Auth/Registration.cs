using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;
using Server.ErrorResults;
using Server.ErrorResults.Errors;

namespace Server.Features.Auth;

public class Registration : ICarterModule
{
    public sealed record Request(
        string Email,
        string Password,
        string ConfirmPassword,
        string Username,
        string? FirstName,
        string? LastName,
        int? Age
    ): IRequest<CustomResult<Response>>;

    public sealed record Response(
        bool IsSuccess,
        IEnumerable<string> RegistrationErrors
    );
    
    public sealed class Validator : AbstractValidator<Request>
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

            RuleFor(c => c.Username)
                .NotEmpty()
                .Matches( "^[a-zA-Z0-9-_]*$")
                .WithMessage("Username must not contain special characters.");
            
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
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/registration", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(request);
                
                return result.Error switch
                {
                    ValidationError => Results.UnprocessableEntity(result.Error.ResultObject),
                    AuthenticationError => Results.BadRequest(result.Error.ResultObject),
                    _ => Results.Ok(result.Value)
                };
            })
            .WithTags("Authentication")
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response>()
            .Produces<Response>(StatusCodes.Status400BadRequest);
    }
    
    internal sealed class Handler(
        IValidator<Request> validator,
        UserManager<CustomIdentityUser> userManager
    ) : IRequestHandler<Request, CustomResult<Response>>
    {
        public async Task<CustomResult<Response>> Handle(Request requesst, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(requesst, cancellationToken);
            if (!validationResult.IsValid)
                return CustomResult<Response>
                    .Failure(new ValidationError(validationResult.GetValidationProblems()));
            
            var user = new CustomIdentityUser
            {
                UserName = requesst.Username,
                Email = requesst.Email,
                FirstName = requesst.FirstName,
                LastName = requesst.LastName,
                Age = requesst.Age
            };

            var createResult = await userManager.CreateAsync(user, requesst.Password);
            
            if (createResult.Succeeded)
                return CustomResult<Response>.Success(new Response(true, []));
            
            var errors = createResult.Errors.Select(e => e.Description);

            return CustomResult<Response>
                .Failure(new AuthenticationError(new Response(false, errors)));
        }
    }
}