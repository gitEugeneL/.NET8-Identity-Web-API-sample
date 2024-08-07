namespace Server.Contracts;

public sealed record ForgotPasswordRequest(
    string Email,
    string ClientUri
);