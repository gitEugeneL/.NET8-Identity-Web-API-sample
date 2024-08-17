using FluentValidation;

namespace Server.Features.Auth.EmailConfirmation;

public sealed class ConfirmationValidator : AbstractValidator<ConfirmationCommand>
{
    public ConfirmationValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.ConfirmationToken)
            .NotEmpty();
    }
}