using System;

namespace Th.Cache.MemoryCache
{
    /// <summary>
    /// 内存缓存接口
    /// </summary>
    internal interface ICustomerMemoryCache
    {
        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <returns>缓存值</returns>
        T Get<T>(string key);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="obj">缓存值</param>
        void Set<T>(string key, T obj);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="obj">缓存值</param>
        /// <param name="timeSpan">过期时间</param>
        void Set<T>(string key, T obj, TimeSpan timeSpan);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="obj">缓存值</param>
        /// <param name="expireTime">过期时间</param>
        void Set<T>(string key, T obj, DateTime expireTime);

        /// <summary>
        /// 删除指定key的缓存值
        /// </summary>
        /// <param name="key">缓存key</param>
        void Remove(string key);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        void Clear();
    }
}
