using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Domain.Entities;
using Server.Services.Interfaces;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Refresh;

internal sealed class RefreshHandler(
    IValidator<RefreshCommand> validator,
    ISecurityService securityService,
    AppDbContext dbContext,
    UserManager<User> userManager) : IRequestHandler<RefreshCommand, Result<RefreshResult>>
{
    public async Task<Result<RefreshResult>> Handle(RefreshCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result<RefreshResult>.Failure(
                new Errors.Validation(validationResult.GetValidationProblems()));
        }
        
        var user = await dbContext
            .Users
            .Include(u => u.RefreshTokens)
            .Where(u => u.RefreshTokens
                .Any(rt => rt.Token == command.RefreshToken))
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            return Result<RefreshResult>.Failure(
                new Errors.Authentication("Authentication error"));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            await userManager.AccessFailedAsync(user);

            return Result<RefreshResult>.Failure(
                new Errors.Authentication("Your account doesn't exist or your password is locked"));
        }
            
        var oldRefreshToken = user
            .RefreshTokens
            .First(rt => rt.Token == command.RefreshToken);

        var devicesCount = user.RefreshTokens.Count;
                    
        if (!securityService.ValidateRefreshToken(oldRefreshToken))
        {
            return Result<RefreshResult>.Failure(
                new Errors.Authentication("Invalid refresh token"));
        }
        
        var userRoles = await userManager.GetRolesAsync(user);
            
        var accessToken = securityService.GenerateAccessToken(user, userRoles);
        var refreshToken = securityService.GenerateRefreshToken(user);
            
        user.RefreshTokens.Remove(oldRefreshToken);
        user.RefreshTokens.Add(refreshToken);

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);
        
        return Result<RefreshResult>.Success(
            new RefreshResult(devicesCount, accessToken, refreshToken.Token, refreshToken.Expires));
    }
}

