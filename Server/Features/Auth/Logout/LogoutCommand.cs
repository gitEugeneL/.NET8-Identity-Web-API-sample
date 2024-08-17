using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result<LogoutResult>>;