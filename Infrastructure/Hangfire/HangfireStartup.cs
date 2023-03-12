using Hangfire.API.Infrastructure.Configs;
using Hangfire.SqlServer;

namespace Hangfire.API.Infrastructure.BackgroundTasks
{
    public static class HangfireStartup
    {
        private const string InterfacePrefix = "I";
        public static void AddCustomHangfire(this IServiceCollection services, IConfiguration configuration)
        {            
            var connectionString = ConnectionString(configuration);            
            var hangfireOptions = new SqlServerStorageOptions
            {
                UseRecommendedIsolationLevel = true,
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                JobExpirationCheckInterval = TimeSpan.FromDays(7),
                QueuePollInterval = TimeSpan.Zero,
                DisableGlobalLocks = true,
                SchemaName = "HangfireDemo"
            };
            
            services.AddHangfire(options => options.UseSqlServerStorage(connectionString, hangfireOptions));
            services.AddHangfireServer(options => 
            {
                options.ServerName = "Hangfire_Server";
            });

            typeof(Startup).Assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Task") && t.IsClass && !t.IsAbstract).ToList()
                .ForEach(t =>
                {
                    var serviceType = t.GetInterface(InterfacePrefix + t.Name);
                    if (serviceType != null) services.AddScoped(serviceType, t);
                });

            services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
            services.AddSingleton<IRecurringJobManager, RecurringJobManager>();

            services.AddHostedService<HangfireHostedService>();            
        }

        private static string ConnectionString(IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection("SqlServerConfig").Get<SqlServerConfig>();
            var connectionString = $"Server={dbConfig.Server};Database={dbConfig.Database};User Id={dbConfig.UserId};Password={dbConfig.Password};TrustServerCertificate=True;Encrypt=True;Max Pool Size=300";

            return connectionString;
        }        
    }           
}
