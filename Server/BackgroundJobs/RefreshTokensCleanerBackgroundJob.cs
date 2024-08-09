using Quartz;
using Server.Data;

namespace Server.BackgroundJobs;

[DisallowConcurrentExecution]
public class RefreshTokensCleanerBackgroundJob(AppDbContext dbContext) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var refreshTokens = dbContext
            .RefreshTokens
            .Where(rt => rt.Expires < DateTime.UtcNow);
        
        dbContext.RemoveRange(refreshTokens);
        dbContext.SaveChanges();
        
        return Task.CompletedTask;
    }
}