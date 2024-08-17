namespace Server.Contracts;

public record RefreshResponse(
    int ConnectedDevices,
    string AccessToken, 
    string RefreshToken, 
    DateTime RefreshTokenExpires, 
    string AccessTokenType = "Bearer"
);