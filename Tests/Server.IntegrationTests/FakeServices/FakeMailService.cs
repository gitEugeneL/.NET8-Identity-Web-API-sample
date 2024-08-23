using Server.Services.Interfaces;

namespace Server.IntegrationTests.FakeServices;

public class FakeMailService : IMailService
{
    public Task<bool> SendMailAsync(string mailTo, string subject, string body) => Task.FromResult(true);
}