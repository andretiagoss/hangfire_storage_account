using Azure.Storage.Blobs.Models;
using Hangfire.API.Infrastructure.AzureStorage;
using Hangfire.API.People.Models;
using Hangfire.Common;
using LinqKit;
using System.ComponentModel;

namespace Hangfire.API.People.BackgroundTasks
{
    public class PersonTask : IPersonTask
    {
        private readonly ILogger<PersonTask> _logger;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IAzureStorage _azureStorage;
        private readonly IConfiguration _configuration;
        private const string JOB_ID = "Remove_Expired_Files";

        public PersonTask(
            ILogger<PersonTask> logger,            
            IRecurringJobManager recurringJobManager,
            IAzureStorage azureStorage,
            IConfiguration configuration)
        {
            _logger = logger;
            _recurringJobManager = recurringJobManager;
            _azureStorage = azureStorage;
            _configuration = configuration;
        }

        public void Install()
        {
            _recurringJobManager.AddOrUpdate(
                JOB_ID,
                Job.FromExpression(() => RemoveFiles()),
                Cron.Daily());
        }

        public void Uninstall()
        {
            _recurringJobManager.RemoveIfExists(JOB_ID);
        }

        [DisplayName("Export all peolple")]
        [AutomaticRetry(Attempts = 2)]
        public async Task ExportAll(string code, string name, string email)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now.ToString()} - Start process to email {email}");

                var personList = Enumerable.Range(1, 10).Select(index => new Person($"000{index}",$"Andre {index}", $"andre{index}@gmail.com"));

                var blobContent = AzureBlobHelper.GenerateCsvString(personList);
                
                var blobName = $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}/{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}_PR00_{code}.csv";
                var uploadResponse = await _azureStorage.UploadAsync(blobContent, blobName);

                if (uploadResponse.Error)
                    throw new Exception(uploadResponse.Status);

                _logger.LogInformation($"{DateTime.Now.ToString()} - End process to email {email}");
                
            }
            catch (Exception ex)
            {
                throw;
            }           
        }

        [DisplayName("Remove Expired Files")]
        [AutomaticRetry(Attempts = 2)]
        public async Task RemoveFiles()
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now.ToString()} - Start process to remove files");

                var retentionRate = _configuration.GetSection("RetentionRateInDays").Get<int>();
                var retentionRateDate = DateTime.UtcNow.AddDays(-retentionRate);

                var blobList = await _azureStorage.ListAsync();

                blobList
                    .Where(a => a.CreatedDate <= retentionRateDate)
                    .ForEach(async r => await _azureStorage.DeleteAsync(r.Name));
                
                _logger.LogInformation($"{DateTime.Now.ToString()} - End process to remove files");                
            }
            catch (Exception ex)
            {
                throw;
            }            
        }
    }
}
