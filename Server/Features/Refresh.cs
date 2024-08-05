using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Server.Contracts;
using Server.Data;
using Server.Security.Interfaces;

namespace Server.Features;

public class Refresh : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/pi/auth/refresh", async (RefreshRequest request, ISender sender) =>
            {
                var command = new Command(request.RefreshToken);
                return await sender.Send(command);
            })
            .WithTags("Authentication")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }

    public sealed record Command(
        string RefreshToken
    ) : IRequest<IResult>;
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty();
        }
    }

    internal sealed class Handler(
        IValidator<Command> validator,
        ISecurityService securityService,
        AppDbContext dbContext
    ) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return Results.UnprocessableEntity(validationResult.GetValidationProblems());

            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .Where(u => u.RefreshTokens
                    .Any(rt => rt.Token == command.RefreshToken))
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null || user.RefreshTokens.Count == 0)
                return Results.Unauthorized();
            
            var oldRefreshToken = user
                .RefreshTokens
                .First(rt => rt.Token == command.RefreshToken);
            
            if (oldRefreshToken.Expires < DateTime.UtcNow)
                return TypedResults.Unauthorized();
            
            var accessToken = securityService.GenerateAccessToken(user);
            var refreshToken = securityService.GenerateRefreshToken(user);
            
            user.RefreshTokens.Remove(oldRefreshToken);
            user.RefreshTokens.Add(refreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return Results.Ok(
                new LoginResponse(accessToken, refreshToken.Token, refreshToken.Expires));
        }
    }
}