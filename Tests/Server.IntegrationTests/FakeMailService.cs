using Server.Services.Interfaces;

namespace Server.IntegrationTests;

public class FakeMailService : IMailService
{
    public Task<bool> SendMailAsync(string mailTo, string subject, string body)
    {
        TestCase.TokenFromEmail = body[body.IndexOf('?')..];
        return Task.FromResult(true);
    }
}