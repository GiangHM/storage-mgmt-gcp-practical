using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StorageManagementAPI.Services
{
    /// <summary>Provides GCP Cloud Storage operations for the application.</summary>
    public interface ICloudStorageService
    {
        /// <summary>Generates a signed URL for a PUT upload to the given object.</summary>
        Task<string> GenerateSignedUrlAsync(string objectName, CancellationToken cancellationToken = default);

        /// <summary>Downloads an object from the configured bucket into <paramref name="destination"/>.</summary>
        Task DownloadObjectAsync(string objectName, Stream destination, CancellationToken cancellationToken = default);

        /// <summary>Uploads a stream to the configured bucket under <paramref name="objectName"/>.</summary>
        Task UploadObjectAsync(string objectName, string contentType, Stream source, CancellationToken cancellationToken = default);
    }
}
