using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken) : IRequest<Result<RefreshResult>>;
