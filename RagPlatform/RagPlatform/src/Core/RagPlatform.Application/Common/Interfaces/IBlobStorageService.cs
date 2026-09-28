namespace RagPlatform.Application.Common.Interfaces;

public interface IBlobStorageService
{
    /// <summary>Uploads a stream to a structured path (tenant/user/year/month/documentId-fileName) and returns the blob path.</summary>
    Task<string> UploadAsync(Stream content, string containerName, string blobPath, string contentType, CancellationToken ct = default);

    Task<Stream> DownloadAsync(string containerName, string blobPath, CancellationToken ct = default);

    Task DeleteAsync(string containerName, string blobPath, CancellationToken ct = default);

    Task<string> ComputeSha256Async(Stream content, CancellationToken ct = default);
}
