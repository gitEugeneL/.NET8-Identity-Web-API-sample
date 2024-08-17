using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResult>>;
