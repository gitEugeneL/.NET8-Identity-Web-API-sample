using MediatR;
using Server.Utils.CustomResult;

namespace Server.Features.Auth.Registration;

public sealed record RegistrationCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string Username,
    string ClientUri,
    string? FirstName = null,
    string? LastName = null,
    int? Age = null) : IRequest<Result<RegistrationResult>>;