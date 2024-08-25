using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class ConfirmationService(UserManager<User> userManager) : IConfirmationService
{
    public async Task<IdentityResult> ConfirmEmail(User user, string confirmationToken) => 
        await userManager.ConfirmEmailAsync(user, confirmationToken);

    public async Task<IdentityResult> ResetPassword(User user, string resetToken, string newPassword) => 
        await userManager.ResetPasswordAsync(user, resetToken, newPassword);
}