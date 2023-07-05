using System.Collections.Concurrent;

namespace ShoppingCart
{
    public interface ICache
    {
        void Add(string key, object value, TimeSpan ttl);
        object? Get(string key);
    }

    public class Cache : ICache
    {
        private static readonly IDictionary<string, (DateTimeOffset, object)> cache = new ConcurrentDictionary<string, (DateTimeOffset, object)>();

        public void Add(string key, object value, TimeSpan ttl)
        {
            cache[key] = (DateTimeOffset.UtcNow.Add(ttl), value);
        }

        public object? Get(string productsResource)
        {
            if (cache.TryGetValue(productsResource, out var value))
                return value;
        
            cache.Remove(productsResource);
            return null;
        }
    }
}
