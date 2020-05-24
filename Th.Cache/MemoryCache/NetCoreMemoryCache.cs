using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

namespace Th.Cache.MemoryCache
{
    /// <summary>
    /// NetCore版本缓存实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NetCoreMemoryCache : ICustomerMemoryCache
    {
        private static IMemoryCache _memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(Options.Create(new MemoryCacheOptions()));

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <returns>缓存值</returns>
        public T Get<T>(string key)
        {
            try
            {
                var isExist = _memoryCache.TryGetValue(key, out var val);
                if (isExist)
                {
                    return (T)val;
                }
                return default(T);
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="obj">缓存值</param>
        public void Set<T>(string key, T obj)
        {
            _memoryCache.GetOrCreate(key, (cacheEntry => obj));
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="obj">缓存值</param>
        /// <param name="timeSpan">过期时间</param>
        public void Set<T>(string key, T obj, TimeSpan timeSpan)
        {
            _memoryCache.GetOrCreate(key, (cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(timeSpan);
                return obj;
            }));
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="obj">缓存值</param>
        /// <param name="expireTime">过期时间</param>
        public void Set<T>(string key, T obj, DateTime expireTime)
        {
            _memoryCache.GetOrCreate(key, (cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(expireTime);
                return obj;
            }));
        }

        /// <summary>
        /// 删除指定key的缓存值
        /// </summary>
        /// <param name="key">缓存key</param>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void Clear()
        {
            _memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
    }
}
