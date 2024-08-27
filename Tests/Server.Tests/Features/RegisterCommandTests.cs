using FluentValidation.TestHelper;
using Server.Features.Auth.Registration;
using Xunit;

namespace Server.Tests.Features;

public class RegistrationCommandTests
{
    private readonly RegistrationValidator _validator = new();

    [Theory]
    [InlineData("email@email.com", "StrongPass1@", "StrongPass1@", "Username123", "https://clientapp.com/api/registrarion")]
    [InlineData("user1@example.com", "Password123!", "Password123!", "User_Name_1", "https://example.com/api/registrarion", "John", "Doe", 25)]
    [InlineData("test.email@domain.com", "SecurePass456$", "SecurePass456$", "User-Name", "https://domain.com/api/registrarion")]
    public void ValidRegistrationCommand_PassesValidation(
        string email,
        string password,
        string confirmPassword,
        string username,
        string clientUri,
        string? firstName = null,
        string? lastName = null,
        int? age = null)
    {
        /*** arrange ***/
        var model = new RegistrationCommand(email, password, confirmPassword, username, clientUri, firstName, lastName, age);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "StrongPass1@", "StrongPass1@", "Username123", "https://clientapp.com")] // Empty Email
    [InlineData("user1@example.com", "short", "short", "User_Name_1", "https://example.com")] // Password too short
    [InlineData("user1@example.com", "Password123!", "DifferentPass123!", "User_Name_1", "https://example.com")] // Passwords do not match
    [InlineData("user1@example.com", "Password123!", "Password123!", "User*Name", "https://example.com")] // Invalid username
    [InlineData("user1@example.com", "Password123!", "Password123!", "Username123", "")] // Empty ClientUri
    [InlineData("user1@example.com", "Password123!", "Password123!", "Username123", "https://example.com", "Jo", "Doe", 25)] // First name too short
    [InlineData("user1@example.com", "Password123!", "Password123!", "Username123", "https://example.com", "John", "D", 25)] // Last name too short
    [InlineData("user1@example.com", "Password123!", "Password123!", "Username123", "https://example.com", "John", "Doe", 17)] // Age too young
    [InlineData("user1@example.com", "Password123!", "Password123!", "Username123", "https://example.com", "John", "Doe", 121)] // Age too old
    public void InvalidRegistrationCommand_FailsValidation(
        string email,
        string password,
        string confirmPassword,
        string username,
        string clientUri,
        string? firstName = null,
        string? lastName = null,
        int? age = null)
    {
        /*** arrange ***/
        var model = 
            new RegistrationCommand(email, password, confirmPassword, username, clientUri, firstName, lastName, age);

        /*** act ***/
        var result = _validator.TestValidate(model);

        /*** assert ***/
        result.ShouldHaveAnyValidationError();
    }
}
