namespace Server.Features.Auth.Login;

public sealed record LoginResult(
     string AccessToken,
     string RefreshToken,
     DateTime ExpireRefreshToken
);