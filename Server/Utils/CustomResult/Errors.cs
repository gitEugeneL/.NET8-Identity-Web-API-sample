namespace Server.Utils.CustomResult;

public abstract class Errors
{
    public record Error(string Code, object Body)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
    }
    
    public sealed record Authentication(object Body) : Error("Authentication.Error", Body);

    public sealed record Validation(object Body) : Error("Validation.Error", Body);

    public sealed record RecordAlreadyExist(object Body) : Error("AlreadyExists.Error", Body);
}
