namespace Server.Contracts;

public record LoginResponse(
     string AccessToken,
     string RefreshToken,
     DateTime RefreshTokenExpires,
     string AccessTokenType = "Bearer"
);