using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Th.Cache.RedisCache
{
    /// <summary>
    /// redis客户端集合
    /// </summary>
    public class RedisCacheClients : Dictionary<int, CSRedisClient>
    {
        private static RedisCacheClients _helper;
        private static readonly object _lock = new object();
        private static List<MRedisSetting> _settings = new List<MRedisSetting>();

        /// <summary>
        /// 隐藏构造函数
        /// </summary>
        private RedisCacheClients()
        {
            _settings = RedisConfigurationManager.GetSettings("RedisSetting");
        }

        /// <summary>
        /// 单例函数
        /// </summary>
        /// <returns></returns>
        public static RedisCacheClients GetSingleton()
        {
            if (_helper == null)
            {
                lock (_lock)
                {
                    if (_helper == null)
                    {
                        try
                        {
                            _helper = new RedisCacheClients();

                            if (_settings != null && _settings.Any())
                            {
                                foreach (MRedisSetting setting in _settings)
                                {
                                    var tmp = setting.ConnectionStr.Split(',');
                                    if (tmp == null || !tmp.Any())
                                    {
                                        // 未找到有效的连接字符串
                                        continue;
                                    }

                                    var tmp1 = tmp.Where(n => !n.Contains("defaultDatabase")).ToList();
                                    string connStr = $"{string.Join(",", tmp1)},defaultDatabase={setting.DbNumber}";

                                    _helper.Add(setting.DbNumber, new CSRedisClient(connStr));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                }
            }

            return _helper;
        }

        public CSRedisClient this[string key]
        {
            get
            {
                int key1 = 0;
                if (!int.TryParse(key, out key1))
                {
                    // 无法转换成db编号，通过别名去查找db编号
                    var tmp = _settings.FirstOrDefault(n => n.DbAlias.Trim() == key);
                    if (tmp == null || string.IsNullOrWhiteSpace(tmp.ConnectionStr))
                    {
                        throw new Exception($"redis数据库{key}未配置连接信息");
                    }

                    key1 = tmp.DbNumber;
                }
                if (!base.ContainsKey(key1))
                {
                    throw new Exception($"redis数据库{key1}未配置连接信息");
                }
                return base[key1];
            }
        }

        public new CSRedisClient this[int key]
        {
            get
            {
                if (!base.ContainsKey(key))
                {
                    throw new Exception($"redis数据库{key}未配置连接信息");
                }
                return base[key];
            }
        }
    }
}
