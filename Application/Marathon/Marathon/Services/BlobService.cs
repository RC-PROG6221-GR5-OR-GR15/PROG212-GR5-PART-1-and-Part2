using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Marathon.Services
{
    /// <summary>Abstraction over Azure Blob Storage so controllers stay testable.</summary>
    public interface IBlobService
    {
        /// <summary>False when no connection string has been configured (uploads disabled).</summary>
        bool IsConfigured { get; }

        /// <summary>Uploads a stream and returns the absolute URL of the stored blob.</summary>
        Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Stores uploaded images (event banners and profile pictures) in an Azure Blob
    /// Storage container, or against the Azurite emulator during development.
    /// </summary>
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient? _container;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureStorage:ConnectionString"];
            var containerName = configuration["AzureStorage:ContainerName"] ?? "raceday-media";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("AzureStorage:ConnectionString is not set. Image uploads are disabled.");
                return;
            }

            var serviceClient = new BlobServiceClient(connectionString);
            _container = serviceClient.GetBlobContainerClient(containerName);
        }

        public bool IsConfigured => _container is not null;

        public async Task<string> UploadAsync(Stream content, string fileName, string contentType,
            CancellationToken cancellationToken = default)
        {
            if (_container is null)
                throw new InvalidOperationException("Azure Blob Storage is not configured.");

            // Create the container lazily so a missing store does not crash startup.
            // Public read access keeps the returned URLs directly usable in <img> tags.
            await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            var safeName = Path.GetFileName(fileName);
            var blobName = $"{Guid.NewGuid():N}_{safeName}";
            var blobClient = _container.GetBlobClient(blobName);

            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType },
                cancellationToken: cancellationToken);

            return blobClient.Uri.ToString();
        }
    }
}
