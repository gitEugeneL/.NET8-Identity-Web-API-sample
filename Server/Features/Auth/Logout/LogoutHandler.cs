using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Logout;

internal sealed class LogoutHandler(
    IValidator<LogoutCommand> validator,
    AppDbContext dbContext) : IRequestHandler<LogoutCommand, Result<LogoutResult>>
{
    public async Task<Result<LogoutResult>> Handle(LogoutCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Result<LogoutResult>.Failure(
                new Errors.Validation(validationResult.GetValidationProblems()));
        }
        
        var user = await dbContext
            .Users
            .Include(u => u.RefreshTokens)
            .Where(u => u.RefreshTokens
                .Any(rt => rt.Token == command.RefreshToken))
            .FirstOrDefaultAsync(ct);

        if (user is null || user.RefreshTokens.Count == 0)
            return Result<LogoutResult>.Failure(new Errors.Authentication("Authentication problem"));
        
        var oldRefreshToken = user
            .RefreshTokens
            .First(rt => rt.Token == command.RefreshToken);
            
        user.RefreshTokens.Remove(oldRefreshToken);

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);

        return Result<LogoutResult>.Success(new LogoutResult(user.Id));
    }
}