using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.EmailConfirmation;

public sealed record ConfirmationCommand(
    string Email,
    string ConfirmationToken
) : IRequest<Result<ConfirmationResult>>;
