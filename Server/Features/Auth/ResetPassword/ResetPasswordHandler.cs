using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;
using Server.Services.Interfaces;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.ResetPassword;

internal sealed class ResetPasswordHandler(
    IValidator<ResetPasswordCommand> validator,
    IConfirmationService confirmationService,
    UserManager<User> userManager) : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResult>>
{
    public async Task<Result<ResetPasswordResult>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result<ResetPasswordResult>.Failure(
                new Errors.Validation(validationResult.GetValidationProblems()));
        }
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return Result<ResetPasswordResult>.Failure(
                new Errors.Authentication("Authentication problem"));
        }
        
        var resetResult = await confirmationService.ResetPassword(user, command.ResetToken, command.NewPassword);
        
        await userManager.SetLockoutEndDateAsync(user, null); // reset lockout and date

        if (resetResult.Succeeded)
            return Result<ResetPasswordResult>.Success(new ResetPasswordResult(user.Id));
            
        var errors = resetResult.Errors.Select(e => e.Description);

        return Result<ResetPasswordResult>.Failure(new Errors.Authentication(new {Errors = errors}));
    }
}