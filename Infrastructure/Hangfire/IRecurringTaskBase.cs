namespace Hangfire.API.Infrastructure.BackgroundTasks
{
    public interface IRecurringTaskBase
    {
        public void Install();
        public void Uninstall();
    }
}
