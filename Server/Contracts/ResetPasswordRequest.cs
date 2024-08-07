namespace Server.Contracts;

public record ResetPasswordRequest(
    string NewPassword,
    string ConfirmNewPassword,
    string Email,
    string ResetToken
);