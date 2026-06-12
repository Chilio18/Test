namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string key, string contentType,
        IDictionary<string, string>? metadata = null, CancellationToken ct = default);

    Task<Stream> DownloadAsync(string key, CancellationToken ct = default);

    Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry, CancellationToken ct = default);

    Task DeleteAsync(string key, CancellationToken ct = default);

    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    Task<long> GetFileSizeAsync(string key, CancellationToken ct = default);
}
