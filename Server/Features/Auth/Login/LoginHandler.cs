using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Domain.Entities;
using Server.Services.Interfaces;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Login;

internal sealed class LoginHandler(
    IValidator<LoginCommand> validator,
    UserManager<User> userManager,
    ISecurityService securityService,
    AppDbContext dbContext) : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(LoginCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result<LoginResult>.Failure(
                new Errors.Validation(validationResult.GetValidationProblems()));
        }
        
        var user = await dbContext
            .Users
            .Include(u => u.RefreshTokens)
            .Where(u => u.NormalizedEmail == command.Email.Trim().ToUpper())
            .SingleOrDefaultAsync(ct);

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            return Result<LoginResult>.Failure(
                new Errors.Authentication("Your account doesn't exist or your password is locked"));
        }
        
        if (!await userManager.CheckPasswordAsync(user, command.Password))
        {
            // set password lockout
            await userManager.AccessFailedAsync(user);

            return Result<LoginResult>.Failure(
                new Errors.Authentication("Your login or password is incorrect"));
        }
        // reset password lockout
        await userManager.ResetAccessFailedCountAsync(user);

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return Result<LoginResult>.Failure(
                new Errors.Authentication("Your email isn't confirmed"));
        }
        
        var userRoles = await userManager.GetRolesAsync(user);
            
        securityService.UpdateRefreshTokenCount(user);
        var accessToken = securityService.GenerateAccessToken(user, userRoles);
        var refreshToken = securityService.GenerateRefreshToken(user);
            
        user.RefreshTokens.Add(refreshToken);
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);
        
        return Result<LoginResult>.Success(
            new LoginResult(accessToken, refreshToken.Token, refreshToken.Expires));
    }
}