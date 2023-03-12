using Hangfire.API.Infrastructure.AzureStorage;
using Hangfire.API.People.Models;
using Hangfire.Common;
using System.ComponentModel;

namespace Hangfire.API.People.BackgroundTasks
{
    public class PersonTask : IPersonTask
    {
        private readonly ILogger<PersonTask> _logger;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IAzureStorage _azureStorage;
        private const string JOB_ID = "Remove_Expired_Files";

        public PersonTask(
            ILogger<PersonTask> logger,            
            IRecurringJobManager recurringJobManager,
            IAzureStorage azureStorage)
        {
            _logger = logger;
            _recurringJobManager = recurringJobManager;
            _azureStorage = azureStorage;
        }

        public void Install()
        {
            _recurringJobManager.AddOrUpdate(
                JOB_ID,
                Job.FromExpression(() => RemoveFiles(30)),
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
                
                var blobName = $"{code}/{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}_PR00_{name}.csv";
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
        public async Task RemoveFiles(int retentionRate)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now.ToString()} - Start process to remove files");

                var blobList = await _azureStorage.ListAsync();

                foreach (var blobItem in blobList)
                {
                    //if (DateTime.Compare(blobItem.CreatedDate.Value.DateTime, DateTime.UtcNow) > retentionRate)
                    //{
                        await _azureStorage.DeleteAsync(blobItem.Name);
                    //}
                }

                _logger.LogInformation($"{DateTime.Now.ToString()} - End process to remove files");                
            }
            catch (Exception ex)
            {
                throw;
            }            
        }
    }
}
