using FluentValidation.TestHelper;
using Server.Features.Auth.Logout;
using Xunit;

namespace Server.Tests.Features;

public class LogoutCommandTests
{
    private readonly LogoutValidator _validator = new();

    [Theory]
    [InlineData("validRefreshToken123")]
    [InlineData("anotherValidRefreshToken456")]
    [InlineData("secureToken789$")]
    public void ValidLogoutCommand_PassesValidation(string refreshToken)
    {
        /*** arrange ***/
        var model = new LogoutCommand(refreshToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void InvalidLogoutCommand_FailsValidation(string refreshToken)
    {
        /*** arrange ***/
        var model = new LogoutCommand(refreshToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}