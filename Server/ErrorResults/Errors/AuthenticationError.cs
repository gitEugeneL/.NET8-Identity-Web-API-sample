namespace Server.ErrorResults.Errors;

public sealed record AuthenticationError(object? ResultObject, string ErrorMessage = "AuthenticationErrorsCode")
    : CustomError(ResultObject, ErrorMessage);
    