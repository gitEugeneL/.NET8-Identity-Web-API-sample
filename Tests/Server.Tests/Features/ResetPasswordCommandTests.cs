using FluentValidation.TestHelper;
using Server.Features.Auth.ResetPassword;
using Xunit;

namespace Server.Tests.Features;

public class ResetPasswordCommandTests
{
    private readonly ResetPasswordValidator _validator = new();

    [Theory]
    [InlineData("NewPassword1@", "NewPassword1@", "email@email.com", "validResetToken123")]
    [InlineData("SecurePass123!", "SecurePass123!", "user1@example.com", "resetToken456")]
    [InlineData("StrongPass@2024", "StrongPass@2024", "test.email@domain.com", "secureToken789$")]
    public void ValidResetPasswordCommand_PassesValidation(string newPassword, string confirmNewPassword, string email, string resetToken)
    {
        /*** arrange ***/
        var model = new ResetPasswordCommand(newPassword, confirmNewPassword, email, resetToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "NewPassword1@", "email@email.com", "validResetToken123")] // Empty NewPassword
    [InlineData("short1@", "short1@", "user1@example.com", "resetToken456")] // NewPassword too short
    [InlineData("NewPassword1@", "DifferentPass1@", "test.email@domain.com", "secureToken789$")] // Passwords do not match
    [InlineData("NewPassword1@", "NewPassword1@", "", "validResetToken123")] // Empty Email
    [InlineData("NewPassword1@", "NewPassword1@", "invalid-email", "validResetToken123")] // Invalid Email format
    [InlineData("NewPassword1@", "NewPassword1@", "email@email.com", "")] // Empty ResetToken
    public void InvalidResetPasswordCommand_FailsValidation(string newPassword, string confirmNewPassword, string email, string resetToken)
    {
        /*** arrange ***/
        var model = new ResetPasswordCommand(newPassword, confirmNewPassword, email, resetToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}