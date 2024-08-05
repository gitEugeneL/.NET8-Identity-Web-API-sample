namespace Server.Contracts;

public sealed record RefreshRequest(
    string RefreshToken
);