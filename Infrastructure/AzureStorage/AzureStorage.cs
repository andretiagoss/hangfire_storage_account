using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Hangfire.API.Infrastructure.AzureStorage.Dtos;
using System.Text;

namespace Hangfire.API.Infrastructure.AzureStorage
{
    public class AzureStorage : IAzureStorage
    {
        private readonly ILogger<AzureStorage> _logger;
        private readonly string _containerName;
        private BlobContainerClient _blobContainerClient;

        public AzureStorage(ILogger<AzureStorage> logger, IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _containerName = configuration.GetSection("AzureStorageConfig:ContainerName").Value;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<List<BlobDto>> ListAsync()
        {            
            var blobDto = new List<BlobDto>();

            await foreach (BlobItem file in _blobContainerClient.GetBlobsAsync())
            {                
                var uri = _blobContainerClient.Uri.ToString();
                var name = file.Name;
                var fullUri = $"{uri}/{name}";
                var createdDate = file.Properties.CreatedOn;

                blobDto.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType,
                    CreatedDate = createdDate
                });
            }
            
            return blobDto;
        }

        public async Task<BlobResponseDto> UploadAsync(string blobContent, string blobName)
        {            
            var response = new BlobResponseDto();

            try
            {                
                var blobClient = _blobContainerClient.GetBlobClient(blobName);                
                byte[] byteArray = Encoding.ASCII.GetBytes(blobContent);

                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(stream);
                }
                
                response.Status = $"File {blobName} Uploaded Successfully";
                response.Error = false;
                response.Blob.Uri = blobClient.Uri.AbsoluteUri;
                response.Blob.Name = blobClient.Name;

            }
            
            catch (RequestFailedException ex)
               when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                var errorMessage = $"File with name {blobName} already exists in container. Set another name to store the file in the container: '{_containerName}.'";
                _logger.LogError(errorMessage);
                response.Status = errorMessage;
                response.Error = true;
                return response;
            }            
            catch (RequestFailedException ex)
            {
                var errorMessage = $"Message: {ex.Message} - StackTrace: {ex.StackTrace}";
                _logger.LogError(errorMessage);
                response.Status = errorMessage;
                response.Error = true;
                return response;
            }
            
            return response;
        }

        public async Task<BlobDto> DownloadAsync(string blobFilename)
        {
            try
            {                
                var blobClient = _blobContainerClient.GetBlobClient(blobFilename);                
                if (await blobClient.ExistsAsync())
                {
                    var data = await blobClient.OpenReadAsync();
                    Stream blobContent = data;
                    
                    var content = await blobClient.DownloadContentAsync();                    
                    var name = blobFilename;
                    var contentType = content.Value.Details.ContentType;                    
                    var blobDto = new BlobDto { Content = blobContent, Name = name, ContentType = contentType };

                    return blobDto;
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {                
                _logger.LogError($"File {blobFilename} was not found.");
            }
            
            return null;
        }

        public async Task<BlobResponseDto> DeleteAsync(string blobFilename)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobFilename);
            try
            {                
                await blobClient.DeleteAsync();
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                var errorMessage = $"File {blobFilename} was not found.";
                _logger.LogError(errorMessage);
                return new BlobResponseDto { Error = true, Status = errorMessage };
            }
            
            return new BlobResponseDto { Error = false, Status = $"File: {blobFilename} has been successfully deleted." };
        }
    }
}
