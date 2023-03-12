namespace Hangfire.API.Infrastructure.AzureStorage.Dtos
{
    public class BlobResponseDto
    {
        public string Status { get; set; }
        public bool Error { get; set; }
        public BlobDto Blob { get; set; }

        public BlobResponseDto()
        {
            Blob = new BlobDto();
        }
    }
}
