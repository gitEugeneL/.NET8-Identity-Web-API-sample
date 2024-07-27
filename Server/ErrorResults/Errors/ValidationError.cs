namespace Server.ErrorResults.Errors;

public record ValidationError(object? ResultObject, string ErrorMessage = "ValidationErrorsCode") 
    : CustomError(ResultObject, ErrorMessage);