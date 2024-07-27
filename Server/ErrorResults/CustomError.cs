namespace Server.ErrorResults;

public record CustomError(object? ResultObject, string ErrorMessage)
{
    public static readonly CustomError None = new(null, string.Empty);
}