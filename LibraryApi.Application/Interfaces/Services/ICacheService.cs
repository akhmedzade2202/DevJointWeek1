namespace LibraryApi.Application.Interfaces.Services;

/// <summary>
/// In-memory cache abstraction for read-heavy endpoints.
/// </summary>
public interface ICacheService
{
    /// <summary>Returns the cached value if present; otherwise invokes the factory, stores the result, and returns it.</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

    /// <summary>Removes one or more cache entries by key prefix or exact key.</summary>
    void Remove(string key);

    /// <summary>Removes all cache entries whose keys start with the given prefix.</summary>
    void RemoveByPrefix(string prefix);
}
