namespace Server.Services.Interfaces;

public interface IMailService
{
    Task<bool> SendMailAsync(string mailTo, string subject, string body);
}