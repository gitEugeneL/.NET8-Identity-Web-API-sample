using Carter;
using Carter.ModelBinding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Server.Contracts;
using Server.Data;
using Server.Helpers;

namespace Server.Features;

public class Logout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/logout", async (RefreshRequest request, ISender sender) =>
            {
                var command = new Command(request.RefreshToken);
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.UserRole)

            .WithTags("Authentication")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    public sealed record Command(
        string RefreshToken
    ) : IRequest<IResult>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.RefreshToken)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(
        IValidator<Command> validator,
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
            
            user.RefreshTokens.Remove(oldRefreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return TypedResults.NoContent();
        }
    }
}