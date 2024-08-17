using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Server.Domain.Entities;
using Server.Services.Interfaces;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.ForgotPassword;

internal sealed class ForgotPasswordHandler(
    IValidator<ForgotPasswordCommand> validator,
    UserManager<User> userManager,
    IMailService mailService) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResult>>
{
    public async Task<Result<ForgotPasswordResult>> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result<ForgotPasswordResult>.Failure(
                new Errors.Validation(validationResult.GetValidationProblems()));
        }
            
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return Result<ForgotPasswordResult>.Failure(
                new Errors.Authentication("Authentication problem"));
        }
        
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var param = new Dictionary<string, string>
        {
            { "resetToken", resetToken },
            { "email", command.Email }
        };
        var callback = QueryHelpers.AddQueryString(command.ClientUri, param);

        await mailService.SendMailAsync(user.Email, "Reset password token", callback);

        return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(user.Id));
    }
}