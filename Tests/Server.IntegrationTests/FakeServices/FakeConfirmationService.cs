using Microsoft.AspNetCore.Identity;
using Server.Data;
using Server.Domain.Entities;
using Server.Services.Interfaces;

namespace Server.IntegrationTests.FakeServices;

public class FakeConfirmationService(AppDbContext dbContext) : IConfirmationService
{
    public async Task<IdentityResult> ConfirmEmail(User user, string confirmationToken)
    {
        user.EmailConfirmed = true;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
        return IdentityResult.Success;
    }
}