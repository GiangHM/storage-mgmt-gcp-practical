using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using storageapi.Infra.gcp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StorageManagementAPI.Services
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly TimeSpan _signedUrlExpiration;
        private readonly StorageClient _storageClient;
        private readonly UrlSigner? _urlSigner;
        private readonly string _bucketName;
        private readonly ILogger<CloudStorageService> _logger;
        private readonly bool _isEmulator;
        private readonly string? _emulatorHost;

        public CloudStorageService(IOptions<GcpOptions> configuration, ILogger<CloudStorageService> logger)
        {
            _logger = logger;
            _bucketName = configuration.Value.StorageBucket
                ?? throw new InvalidOperationException("GCP:StorageBucket configuration is required.");

            _signedUrlExpiration = TimeSpan.FromMinutes(
                configuration.Value.SignedUrlExpirationMinutes);

            _emulatorHost = Environment.GetEnvironmentVariable("STORAGE_EMULATOR_HOST");
            _isEmulator = !string.IsNullOrEmpty(_emulatorHost);

            if (_isEmulator)
            {
                _logger.LogInformation("Using Storage Emulator at {EmulatorHost}", _emulatorHost);
                _storageClient = new StorageClientBuilder
                {
                    BaseUri = _emulatorHost,
                    UnauthenticatedAccess = true,
                }.Build();
                _urlSigner = null;
            }
            else
            {
                _storageClient = StorageClient.Create();
                _urlSigner = UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateSignedUrlAsync(string objectName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating signed URL for object {ObjectName} in bucket {Bucket}", objectName, _bucketName);

            if (_isEmulator)
            {
                // Return a path the Vite dev server proxies to fake-gcs to avoid browser CORS.
                // fake-gcs requires uploadType=multipart (it does not support uploadType=media).
                // Object name goes in the multipart body metadata, not the URL.
                return $"/storage-proxy/upload/storage/v1/b/{_bucketName}/o?uploadType=multipart";
            }

            var requestTemplate = UrlSigner.RequestTemplate
                .FromBucket(_bucketName)
                .WithObjectName(objectName)
                .WithHttpMethod(HttpMethod.Put);

            var options = UrlSigner.Options.FromDuration(_signedUrlExpiration);

            return await _urlSigner!.SignAsync(requestTemplate, options, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DownloadObjectAsync(string objectName, Stream destination, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Downloading object {ObjectName} from bucket {Bucket}", objectName, _bucketName);
            await _storageClient.DownloadObjectAsync(_bucketName, objectName, destination, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UploadObjectAsync(string objectName, string contentType, Stream source, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(contentType, nameof(contentType));
            _logger.LogInformation("Uploading object {ObjectName} to bucket {Bucket}", objectName, _bucketName);
            await _storageClient.UploadObjectAsync(_bucketName, objectName, contentType, source, cancellationToken: cancellationToken);
        }
    }
}
