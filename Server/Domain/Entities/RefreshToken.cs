namespace Server.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; init; }
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }

    /*** Relations ***/
    public required User User { get; init; }
    public string UserId { get; init; }
}