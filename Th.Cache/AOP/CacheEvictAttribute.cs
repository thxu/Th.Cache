using ArxOne.MrAdvice.Advice;
using System;
using CSRedis;
using Th.Cache.MemoryCache;
using Th.Cache.RedisCache;

namespace Th.Cache.AOP
{
    /// <summary>
    /// 缓存删除
    /// </summary>
    public class CacheEvictAttribute : Attribute, IMethodAdvice
    {
        private readonly string _redisDb;
        private readonly string _key;
        private readonly CacheType _cacheType;
        private CSRedisClient _redisClient = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">缓存的key，默认使用当前参数的md5值作为key，该key会自动加上Go_AutoCache_前缀
        /// 也可使用函数入参为key，将函数入参以{开头，以}结尾。
        /// 如
        /// "{id}"即为将入参id作为key;
        /// "{user.id}"即为将入参user对象的id属性作为key;
        /// "{param1}-{param2.id}"即为将入参param1,param2.id用'-'拼接后的字符串作为key</param>
        /// <param name="cacheType">缓存方式,默认使用本地缓存</param>
        /// <param name="redisDb">redis数据库，缓存方式为redis时必传</param>
        public CacheEvictAttribute(CacheType cacheType = CacheType.MemoryCache, string key = "", string redisDb = "")
        {
            _cacheType = cacheType;
            _key = key;
            _redisDb = redisDb;
            if (!string.IsNullOrWhiteSpace(_redisDb))
            {
                _redisClient = RedisCacheClients.GetSingleton()[_redisDb];
            }
        }

        /// <summary>
        /// Implements advice logic.
        /// Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
            var key = _key.GetKey(context);
            if (string.IsNullOrWhiteSpace(key))
            {
                // 没有生成key，此时不知道清除缓存的key，故不清除缓存数据
                return;
            }

            if (_cacheType == CacheType.MemoryCache)
            {
                MemoryCacheHelper.Remove(key);
            }
            else
            {
                var localKeyTmp1 = key + "1";
                var localKeyTmp2 = key + "2";
                MemoryCacheHelper.Remove(key);
                MemoryCacheHelper.Remove(localKeyTmp1);
                MemoryCacheHelper.Remove(localKeyTmp2);
                if (!string.IsNullOrWhiteSpace(_redisDb))
                {
                    _redisClient.Del(key);
                }
            }
        }
    }
}
