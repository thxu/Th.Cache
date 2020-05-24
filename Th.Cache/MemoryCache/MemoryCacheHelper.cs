using System;

namespace Th.Cache.MemoryCache
{
    public static class MemoryCacheHelper
    {
        private static ICustomerMemoryCache _cacheClient = null;

        static MemoryCacheHelper()
        {
#if NETFRAMEWORK
            _cacheClient = new NetFrameworkMemoryCache();
#else
            _cacheClient = new NetCoreMemoryCache();
#endif
        }

        public static T Get<T>(string key)
        {
            return _cacheClient.Get<T>(key);
        }

        public static void Set<T>(string key, T obj)
        {
            _cacheClient.Set<T>(key, obj);
        }

        public static void Set<T>(string key, T obj, TimeSpan timeSpan)
        {
            _cacheClient.Set<T>(key, obj, timeSpan);
        }

        public static void Set<T>(string key, T obj, DateTime expireTime)
        {
            _cacheClient.Set<T>(key, obj, expireTime);
        }

        public static void Remove(string key)
        {
            _cacheClient.Remove(key);
        }

        public static void Clear()
        {
            _cacheClient.Clear();
        }
    }
}
