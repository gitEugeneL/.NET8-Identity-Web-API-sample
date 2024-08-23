using Microsoft.AspNetCore.Identity;
using Server.Domain.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class ConfirmationService(UserManager<User> userManager) : IConfirmationService
{
    public async Task<IdentityResult> ConfirmEmail(User user, string confirmationToken)
    {
        return await userManager.ConfirmEmailAsync(user, confirmationToken);
    }
}