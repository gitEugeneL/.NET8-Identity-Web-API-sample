using FluentValidation;

namespace Server.Features.Auth.Registration;

public sealed class RegistrationValidator : AbstractValidator<RegistrationCommand>
{
    public RegistrationValidator()
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