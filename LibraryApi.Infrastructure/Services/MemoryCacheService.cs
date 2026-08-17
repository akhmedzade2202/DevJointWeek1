using LibraryApi.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Infrastructure.Services;

/// <summary>
/// In-memory cache implementation backed by <see cref="IMemoryCache"/>.
/// Keeps a registry of active keys so prefix-based invalidation is possible.
/// Default TTL values are read from <c>Cache:AbsoluteExpirationMinutes</c> and
/// <c>Cache:SlidingExpirationMinutes</c> in configuration.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly TimeSpan _defaultAbsoluteExpiration;
    private readonly TimeSpan _defaultSlidingExpiration;

    // Registry of all keys currently stored so RemoveByPrefix can work.
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger, IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;

        var absoluteMinutes = configuration.GetValue<int>("Cache:AbsoluteExpirationMinutes", 10);
        var slidingMinutes = configuration.GetValue<int>("Cache:SlidingExpirationMinutes", 5);

        _defaultAbsoluteExpiration = TimeSpan.FromMinutes(absoluteMinutes);
        _defaultSlidingExpiration = TimeSpan.FromMinutes(slidingMinutes);

        _logger.LogInformation(
            "MemoryCacheService initialised — AbsoluteExpiration: {Abs} min, SlidingExpiration: {Slide} min",
            absoluteMinutes, slidingMinutes);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null)
    {
        if (_cache.TryGetValue(key, out T? cached))
        {
            _logger.LogDebug("Cache HIT for key: {Key}", key);
            return cached!;
        }

        _logger.LogDebug("Cache MISS for key: {Key} — fetching from source", key);
        var value = await factory();

        var options = new MemoryCacheEntryOptions();

        // Use caller-supplied TTL if provided, otherwise fall back to config-driven defaults.
        options.AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultAbsoluteExpiration;
        options.SlidingExpiration = slidingExpiration ?? _defaultSlidingExpiration;

        // Clean up the key from the registry when it expires/evicts.
        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            lock (_lock)
            {
                _keys.Remove(evictedKey.ToString()!);
            }
        });

        _cache.Set(key, value, options);

        lock (_lock)
        {
            _keys.Add(key);
        }

        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        lock (_lock)
        {
            _keys.Remove(key);
        }
        _logger.LogDebug("Cache REMOVED key: {Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        List<string> toRemove;
        lock (_lock)
        {
            toRemove = _keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        foreach (var key in toRemove)
        {
            _cache.Remove(key);
            lock (_lock)
            {
                _keys.Remove(key);
            }
        }

        _logger.LogDebug("Cache INVALIDATED {Count} keys with prefix: {Prefix}", toRemove.Count, prefix);
    }
}
