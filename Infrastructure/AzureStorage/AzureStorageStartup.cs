using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

namespace Hangfire.API.Infrastructure.AzureStorage
{
    public static class AzureStorageStartup
    {
        public static IServiceCollection AddCustomAzureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureClients(cfg =>
            {
                cfg.AddBlobServiceClient(configuration.GetSection("AzureStorageConfig"))
                .WithCredential(new DefaultAzureCredential());
            });
            
            services.AddTransient<IAzureStorage, AzureStorage>();

            return services;
        }
    }
}
