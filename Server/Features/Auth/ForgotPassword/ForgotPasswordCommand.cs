using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email,
    string ClientUri) : IRequest<Result<ForgotPasswordResult>>;