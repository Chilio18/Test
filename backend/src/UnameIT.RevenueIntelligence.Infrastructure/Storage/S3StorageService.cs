using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;

namespace UnameIT.RevenueIntelligence.Infrastructure.Storage;

public class S3StorageOptions
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string? ServiceUrl { get; set; }
    public string Region { get; set; } = "eu-west-1";
}

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly S3StorageOptions _options;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IOptions<S3StorageOptions> options, ILogger<S3StorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region) };
        if (!string.IsNullOrEmpty(_options.ServiceUrl))
        {
            config.ServiceURL = _options.ServiceUrl;
            config.ForcePathStyle = true;
        }

        _s3 = new AmazonS3Client(
            new BasicAWSCredentials(_options.AccessKey, _options.SecretKey),
            config);
    }

    public async Task<string> UploadAsync(Stream fileStream, string key, string contentType,
        IDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        if (metadata != null)
            foreach (var kv in metadata)
                request.Metadata[kv.Key] = kv.Value;

        await _s3.PutObjectAsync(request, ct);
        return key;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken ct = default)
    {
        var response = await _s3.GetObjectAsync(_options.BucketName, key, ct);
        return response.ResponseStream;
    }

    public async Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry,
        CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET
        };
        return await Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
        => await _s3.DeleteObjectAsync(_options.BucketName, key, ct);

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _s3.GetObjectMetadataAsync(_options.BucketName, key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<long> GetFileSizeAsync(string key, CancellationToken ct = default)
    {
        var metadata = await _s3.GetObjectMetadataAsync(_options.BucketName, key, ct);
        return metadata.ContentLength;
    }
}
