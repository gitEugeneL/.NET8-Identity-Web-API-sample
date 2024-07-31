using MediatR;

namespace Server.Contracts;

public sealed record RegistrationRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string Username,
    string? FirstName,
    string? LastName,
    int? Age
);