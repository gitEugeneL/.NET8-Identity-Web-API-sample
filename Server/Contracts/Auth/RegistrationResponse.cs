namespace Server.Contracts.Auth;

public sealed record RegistrationResponse(
    bool IsSuccess,
    IEnumerable<string> RegistrationErrors
);