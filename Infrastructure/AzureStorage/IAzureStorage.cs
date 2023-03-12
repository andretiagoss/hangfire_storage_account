using Hangfire.API.Infrastructure.AzureStorage.Dtos;

namespace Hangfire.API.Infrastructure.AzureStorage
{
    public interface IAzureStorage
    {        
        Task<BlobResponseDto> UploadAsync(string blobContent, string blobName);        
        Task<BlobDto> DownloadAsync(string blobFilename);        
        Task<BlobResponseDto> DeleteAsync(string blobFilename);        
        Task<List<BlobDto>> ListAsync();
    }
}
