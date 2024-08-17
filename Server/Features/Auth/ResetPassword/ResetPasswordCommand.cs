using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(
    string NewPassword,
    string ConfirmNewPassword,
    string Email,
    string ResetToken ) : IRequest<Result<ResetPasswordResult>>;
