using Th.Cache.RedisCache;

namespace Th.Cache.NetFramework.TestExe
{
    public class RedisCacheTest
    {
        public bool AddTest()
        {
            var key = "redis_Key_1";
            var val = "hello";
            RedisCacheClients.GetSingleton()[1].Set(key, val);

            var valInRedis = RedisCacheClients.GetSingleton()[1].Get<string>(key);
            return val == valInRedis;
        }
    }
}
