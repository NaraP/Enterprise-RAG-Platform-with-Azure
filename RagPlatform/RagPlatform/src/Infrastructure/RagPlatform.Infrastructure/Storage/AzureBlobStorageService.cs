using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Infrastructure.Storage;

/// <summary>Wraps Azure Storage Account blob operations. Auth via Azure.Identity
/// (managed identity in Azure, az login locally) — configured in DependencyInjection.</summary>
public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient) => _blobServiceClient = blobServiceClient;

    public async Task<string> UploadAsync(Stream content, string containerName, string blobPath, string contentType, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blob = container.GetBlobClient(blobPath);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        return blobPath;
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobPath, CancellationToken ct = default)
    {
        var blob = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobPath);
        var response = await blob.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string blobPath, CancellationToken ct = default)
    {
        var blob = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobPath);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task<string> ComputeSha256Async(Stream content, CancellationToken ct = default)
    {
        content.Position = 0;
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(content, ct);
        content.Position = 0;
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
