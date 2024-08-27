using FluentValidation.TestHelper;
using Server.Features.Auth.Refresh;
using Xunit;

namespace Server.Tests.Features;

public class RefreshCommandTests
{
    private readonly RefreshValidator _validator = new();

    [Theory]
    [InlineData("validRefreshToken123")]
    [InlineData("anotherValidToken456")]
    [InlineData("secureRefreshToken789$")]
    public void ValidRefreshCommand_PassesValidation(string refreshToken)
    {
        /*** arrange ***/
        var model = new RefreshCommand(refreshToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void InvalidRefreshCommand_FailsValidation(string refreshToken)
    {
        /*** arrange ***/
        var model = new RefreshCommand(refreshToken);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}