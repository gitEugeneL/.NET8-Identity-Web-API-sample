using FluentValidation.TestHelper;
using Server.Features.Auth.ForgotPassword;
using Xunit;

namespace Server.Tests.Features;

public class ForgotPasswordCommandTests
{
    private readonly ForgotPasswordValidator _validator = new();

    [Theory]
    [InlineData("email@email.com", "https://clientapp.com/reset-password")]
    [InlineData("user1@example.com", "https://example.com/reset-password")]
    [InlineData("test.email@domain.com", "https://domain.com/reset-password")]
    public void ValidForgotPasswordCommand_PassesValidation(string email, string clientUri)
    {
        /*** arrange ***/
        var model = new ForgotPasswordCommand(email, clientUri);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "https://clientapp.com/reset-password")] // Empty Email
    [InlineData("user1@example.com", "")] // Empty ClientUri
    [InlineData("notanemail", "https://example.com/reset-password")] // Invalid Email format
    public void InvalidForgotPasswordCommand_FailsValidation(string email, string clientUri)
    {
        /*** arrange ***/
        var model = new ForgotPasswordCommand(email, clientUri);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}