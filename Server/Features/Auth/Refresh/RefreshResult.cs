namespace Server.Features.Auth.Refresh;

public sealed record RefreshResult(
    int ConnectedDevices,
    string AccessToken,
    string RefreshToken,
    DateTime ExpireRefreshToken
);