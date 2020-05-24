#if NETFRAMEWORK
using System;
using System.Collections;
using System.Web;

namespace Th.Cache.MemoryCache
{
    public class NetFrameworkMemoryCache : ICustomerMemoryCache
    {
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
                System.Web.Caching.Cache objCache = HttpRuntime.Cache;
                var val = objCache[key];
                if (val == null)
                {
                    return default(T);
                }
                return (T)val;
            }
            catch (Exception)
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
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(key, obj);
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
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(key, obj, null, DateTime.Now.Add(timeSpan), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.NotRemovable, null);
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
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(key, obj, null, expireTime, TimeSpan.Zero, System.Web.Caching.CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 删除指定key的缓存值
        /// </summary>
        /// <param name="key">缓存key</param>
        public void Remove(string key)
        {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            cache.Remove(key);
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void Clear()
        {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key != null) cache.Remove(cacheEnum.Key.ToString());
            }
        }
    }
}
#else
#endif
