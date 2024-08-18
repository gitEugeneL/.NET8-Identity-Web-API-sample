namespace Server.Contracts;

public sealed record RegistrationRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string Username,
    string ClientUri,
    string? FirstName = null,
    string? LastName = null,
    int? Age = null
);