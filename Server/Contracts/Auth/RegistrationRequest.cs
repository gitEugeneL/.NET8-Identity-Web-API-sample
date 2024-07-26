namespace Server.Contracts.Auth;

public sealed record RegistrationRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string? FirstName = null,
    string? LastName = null,
    int? Age = null
);