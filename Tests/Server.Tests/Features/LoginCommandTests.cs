using FluentValidation.TestHelper;
using Server.Features.Auth.Login;
using Xunit;

namespace Server.Tests.Features;

public class LoginCommandTests
{
    private readonly LoginValidator _validator = new();

    [Theory]
    [InlineData("email@email.com", "strongPassword1@")]
    [InlineData("user1@example.com", "Password123!")]
    [InlineData("test.email@domain.com", "SecurePass456$")]
    public void ValidLoginCommand_PassesValidation(string email, string password)
    {
        /*** arrange ***/
        var model = new LoginCommand(email, password);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "password")] // Empty Email
    [InlineData("test@example.com", "")] // Empty Password
    [InlineData("notanemail", "password")] // Invalid Email format
    public void InvalidLoginCommand_FailsValidation(string email, string password)
    {
        /*** arrange ***/
        var model = new LoginCommand(email, password);

        /*** act ***/
        var result = _validator.TestValidate(model);
        
        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}