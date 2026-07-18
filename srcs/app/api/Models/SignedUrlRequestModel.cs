using System.ComponentModel.DataAnnotations;

namespace storageapi.Models
{
    public record SignedUrlRequestModel
    {
        [Required]
        public string DocumentId { get; init; } = string.Empty;

        [Required]
        public string FileName { get; init; } = string.Empty;
    }
}
