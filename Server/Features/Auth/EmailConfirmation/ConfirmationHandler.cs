using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.EmailConfirmation;

internal sealed class ConfirmationHandler(
    IValidator<ConfirmationCommand> validator,
    UserManager<User> userManager) : IRequestHandler<ConfirmationCommand, Result<ConfirmationResult>>
{
    public async Task<Result<ConfirmationResult>> Handle(ConfirmationCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Result<ConfirmationResult>.Failure(new Errors.Validation(validationResult.GetValidationProblems()));
        
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result<ConfirmationResult>.Failure(new Errors.Authentication("Authentication problem"));
        
        var confirmResult = await userManager.ConfirmEmailAsync(user, command.ConfirmationToken);

        return confirmResult.Succeeded
            ? Result<ConfirmationResult>.Success(new ConfirmationResult(user.Id))
            : Result<ConfirmationResult>.Failure(new Errors.Authentication("Invalid confirmation token"));
    }
}