namespace storageapi.Models
{
    public record SignedUrlResponseModel
    {
        public string UploadUrl { get; init; } = string.Empty;
        public string ObjectName { get; init; } = string.Empty;
    }
}
