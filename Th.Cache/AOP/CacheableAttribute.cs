using ArxOne.MrAdvice.Advice;
using CSRedis;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Th.Cache.MemoryCache;
using Th.Cache.RedisCache;

namespace Th.Cache.AOP
{
    /// <summary>
    /// 自动缓存特性
    /// </summary>
    public class CacheableAttribute : Attribute, IMethodAdvice
    {
        private readonly string _redisDb;
        private readonly string _key;
        private readonly CacheType _cacheType;
        private readonly int _expireTime;
        private CSRedisClient _redisClient = null;

        //private string _condition;
        //private string _unless;

        private static readonly object LockObj = new object();

        /// <summary>
        /// 双缓存启用限制条件（5000即缓存过期时间大于5000ms的才启用双缓存机制）
        /// </summary>
        private double _doubleCacheLimit = 5000;

        /// <summary>
        /// 本地临时缓存过期时间
        /// </summary>
        private double _tmpLocalCacheLimit = 500;

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
        /// <param name="expireTime">过期时间，单位毫秒(ms)，小于或等于零即表示永不过期，默认过期时间为3000毫秒</param>
        /// <param name="redisDb">redis数据库，缓存方式为redis时必传</param>
        public CacheableAttribute(CacheType cacheType = CacheType.MemoryCache, string key = "", int expireTime = 3000, string redisDb = "")
        {
            _cacheType = cacheType;
            _key = key;
            _expireTime = expireTime;
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
            if (context.TargetMethod.IsDefined(typeof(CacheEvictAttribute), true))
            {
                // 定义了缓存清除特性，优先使用缓存清除特性
                context.Proceed();
                return;
            }

            if (!context.HasReturnValue)
            {
                // 没有返回值，直接执行方法并返回，无需缓存
                context.Proceed();
                return;
            }

            var key = _key.GetKey(context);
            if (string.IsNullOrWhiteSpace(key))
            {
                // 没有生成key，此时不知道缓存的key，故不缓存数据
                context.Proceed();
                return;
            }
            if (_cacheType == CacheType.MemoryCache)
            {
                // 使用本地内存缓存数据
                //var catchRes = HttpRuntime.Cache[key];
                var catchRes = MemoryCacheHelper.Get<object>(key);
                if (catchRes != null)
                {
                    // 缓存中找到了数据，直接返回
                    context.ReturnValue = catchRes;
                    return;
                }
                // 缓存中没有数据，执行方法后将结果写入缓存
                ProceedWithLock(context, key);
                InsertLocalCache(key, context.ReturnValue, _expireTime);
            }
            else
            {
                // 使用redis缓存数据
                if (string.IsNullOrWhiteSpace(_redisDb))
                {
                    // 没指定用哪个redis实例链接，不缓存数据
                    context.Proceed();
                    return;
                }
                // 为了避免缓存雪崩（即数据不存在时大量线程访问该方法导致所有请求都到了数据库），本方法使用了
                // 1、双缓存策略，即在查询到数据即将过期时将当前数据多缓存一份到内存中，并异步执行方法，获取到返回结果后刷新redis缓存并删除内存中的缓存
                // 2、加锁排队策略，即缓存中没有数据时，在查询数据库之前先加锁，查询完毕后解锁，优化：获取到锁，查询数据库之后将数据缓存1秒，使下一个获取到锁的线程不用去查询数据库，直接从缓存中获取。

                // 优化：加了一个过期时间为500毫秒的内地内存中的缓存，用于优化短时间内大量调用同一个查询接口的情况，此时直接使用本地内存中缓存的数据，不去查询redis中的数据了。
                var localKeyTmp = key + "2";
                //var localCatchResTmp = HttpRuntime.Cache[localKeyTmp];
                var localCatchResTmp = MemoryCacheHelper.Get<object>(localKeyTmp);
                if (localCatchResTmp != null)
                {
                    // 缓存中找到了数据，直接返回
                    context.ReturnValue = localCatchResTmp;
                    return;
                }

                // 使用双缓存策略中的本地内存缓存数据
                if (_expireTime > _doubleCacheLimit)
                {
                    //var localCatchRes = HttpRuntime.Cache[key];
                    var localCatchRes = MemoryCacheHelper.Get<object>(key);
                    if (localCatchRes != null)
                    {
                        // 缓存中找到了数据，直接返回
                        context.ReturnValue = localCatchRes;
                        InsertLocalCache(localKeyTmp, context.ReturnValue, _tmpLocalCacheLimit);
                        return;
                    }
                }

                // 获取redis中缓存的数据
                var catchRes = _redisClient.Get<string>(key);
                if (!string.IsNullOrWhiteSpace(catchRes))
                {
                    var methodInfo = context.TargetMethod as MethodInfo;
                    if (methodInfo != null)
                    {
                        var tmp = JsonConvert.DeserializeObject(catchRes, methodInfo.ReturnType);
                        if (tmp != null)
                        {
                            // 缓存中找到了数据，直接返回
                            context.ReturnValue = tmp;

                            // 异步查询数据过期情况，若数据即将过期，则启动双缓存策略
                            if (_expireTime > _doubleCacheLimit)
                            {
                                Task.Factory.StartNew((() =>
                                {
                                    var milliseconds = _redisClient.PTtl(key);
                                    double timeLimit = _expireTime / 5;
                                    if (timeLimit > 60000)
                                    {
                                        timeLimit = 60000;
                                    }
                                    if (milliseconds <= timeLimit)
                                    {
                                        // 过期时间小于阈值（设定过期时间的1/5，最多60s），复制一份数据到内存缓存中，并执行方法获取最新的数据
                                        InsertLocalCache(key, context.ReturnValue, 10000);
                                        if (ProceedWithLock(context, key))
                                        {
                                            if (_expireTime <= 0)
                                            {
                                                _redisClient.Set(key, context.ReturnValue.SerializeObject());
                                            }
                                            else
                                            {
                                                _redisClient.Set(key, context.ReturnValue.SerializeObject(), _expireTime);
                                            }
                                        }

                                        //HttpRuntime.Cache.Remove(key);
                                        MemoryCacheHelper.Remove(key);
                                    }
                                }));
                            }
                            InsertLocalCache(localKeyTmp, context.ReturnValue, _tmpLocalCacheLimit);

                            return;
                        }
                    }
                }

                // 缓存中没有数据，执行方法后将结果写入缓存，
                if (ProceedWithLock(context, key))
                {
                    if (_expireTime <= 0)
                    {
                        _redisClient.Set(key, context.ReturnValue.SerializeObject());
                    }
                    else
                    {
                        _redisClient.Set(key, context.ReturnValue.SerializeObject(), _expireTime);
                    }
                }

                InsertLocalCache(localKeyTmp, context.ReturnValue, _tmpLocalCacheLimit);
            }
        }

        private bool ProceedWithLock(MethodAdviceContext context, string key)
        {
            var keyTmp = key + "1";
            lock (LockObj)
            {
                var localCatchRes = MemoryCacheHelper.Get<object>(keyTmp);
                if (localCatchRes != null)
                {
                    // 缓存中找到了数据，直接返回
                    context.ReturnValue = localCatchRes;
                    return true;
                }
                context.Proceed();
                InsertLocalCache(keyTmp, context.ReturnValue, 500);
            }

            return false;
        }

        private void InsertLocalCache(string key, object val, double milliseconds)
        {
            if (milliseconds <= 0)
            {
                MemoryCacheHelper.Set(key, val);
                return;
            }
            MemoryCacheHelper.Set(key, val, DateTime.Now.AddMilliseconds(milliseconds));
        }
    }

    /// <summary>
    /// 指定缓存方式
    /// </summary>
    [Serializable]
    public enum CacheType
    {
        /// <summary>
        /// 内存缓存
        /// </summary>
        MemoryCache = 1,

        /// <summary>
        /// redis缓存
        /// </summary>
        Redis = 2,
    }
}
