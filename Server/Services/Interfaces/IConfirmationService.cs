using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;

namespace Server.Services.Interfaces;

public interface IConfirmationService
{
    Task<IdentityResult> ConfirmEmail(User user, string confirmationToken);

    Task<IdentityResult> ResetPassword(User user, string resetToken, string newPassword);
}