using FluentValidation.TestHelper;
using Server.Features.Auth.EmailConfirmation;
using Xunit;

namespace Server.Tests.Features;

public class ConfirmationCommandTests
{
    private readonly ConfirmationValidator _validator = new();

    [Theory]
    [InlineData("email@email.com", "validToken123")]
    [InlineData("user1@example.com", "Token456!")]
    [InlineData("test.email@domain.com", "AnotherSecureToken789$")]
    public void ValidConfirmationCommand_PassesValidation(string email, string confirmationToken)
    {
        /*** arrange ***/
        var model = new ConfirmationCommand(email, confirmationToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "validToken123")] // Empty Email
    [InlineData("user1@example.com", "")] // Empty Confirmation Token
    [InlineData("notanemail", "validToken123")] // Invalid Email format
    public void InvalidConfirmationCommand_FailsValidation(string email, string confirmationToken)
    {
        /*** arrange ***/
        var model = new ConfirmationCommand(email, confirmationToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}