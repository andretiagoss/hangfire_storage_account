using Hangfire.API.People.BackgroundTasks;

namespace Hangfire.API.Infrastructure.BackgroundTasks
{
    public class HangfireHostedService : IHostedService
    {
        private readonly ILogger<HangfireHostedService> _logger;        
        private readonly IServiceScopeFactory _serviceScopeFactory;        

        public HangfireHostedService(
            ILogger<HangfireHostedService> logger,             
            IServiceScopeFactory serviceScopeFactory
            )
        {            
            _logger = logger;            
            _serviceScopeFactory = serviceScopeFactory;            
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start app");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var scopedService = scope.ServiceProvider.GetService<IPersonTask>();
                scopedService.Install();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop app");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var scopedService = scope.ServiceProvider.GetService<IPersonTask>();
                scopedService.Uninstall();
            }

            return Task.CompletedTask;
        }        
    }
}
