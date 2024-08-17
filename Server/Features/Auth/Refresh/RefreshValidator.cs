using FluentValidation;

namespace Server.Features.Auth.Refresh;

public sealed class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();
    }
}