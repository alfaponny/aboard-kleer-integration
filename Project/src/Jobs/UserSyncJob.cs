using Quartz;
using AboardKleerIntegration.Sync;


[DisallowConcurrentExecution] 
public class UserSyncJob(UserSyncService _sync) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await _sync.SyncUsersAsync();
    }
}