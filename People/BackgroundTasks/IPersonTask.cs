using Hangfire.API.Infrastructure.BackgroundTasks;

namespace Hangfire.API.People.BackgroundTasks
{
    public interface IPersonTask : IRecurringTaskBase
    {
        Task ExportAll(string code, string name, string email);
        Task RemoveFiles(int retentionRate);
    }
}
