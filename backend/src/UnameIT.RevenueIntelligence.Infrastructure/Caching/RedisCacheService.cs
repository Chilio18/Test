using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;

namespace UnameIT.RevenueIntelligence.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        try { return JsonSerializer.Deserialize<T>(value!, JsonOptions); }
        catch (Exception ex) { _logger.LogWarning(ex, "Cache deserialize error for key {Key}", key); return default; }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromHours(1));
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(key);

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null,
        CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached != null) return cached;
        var value = await factory();
        if (value != null) await SetAsync(key, value, expiry, ct);
        return value!;
    }

    public async Task InvalidateByPatternAsync(string pattern, CancellationToken ct = default)
    {
        var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0) await _db.KeyDeleteAsync(keys);
    }
}
