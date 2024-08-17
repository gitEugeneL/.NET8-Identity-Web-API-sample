using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Server.Domain.Entities;
using Server.Helpers;
using Server.Services.Interfaces;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Registration;

internal sealed class RegistrationHandler(
    IValidator<RegistrationCommand> validator,
    UserManager<User> userManager,
    IMailService mailService) : IRequestHandler<RegistrationCommand, Result<RegistrationResult>>
{
    public async Task<Result<RegistrationResult>> Handle(RegistrationCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result<RegistrationResult>.Failure(
                new Errors.Validation(validationResult.GetValidationProblems()));
        }
        
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
        {
            return Result<RegistrationResult>.Failure(
                new Errors.Authentication(createResult.Errors.Select(e => e.Description)));
        }
        
        await userManager.AddToRoleAsync(user, Roles.User);
            
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var param = new Dictionary<string, string>
        {
            { "confirmationToken", confirmationToken },
            { "email", user.Email }
        };
        var callback = QueryHelpers.AddQueryString(command.ClientUri, param);
            
        await mailService.SendMailAsync(user.Email, "Email Confirmation token", callback);

        return Result<RegistrationResult>.Success(
            new RegistrationResult(user.Id));
    }
}