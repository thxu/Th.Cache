using System;
using Th.Cache.RedisCache;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Configuration;
#else
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
#endif

namespace Th.Cache
{
    internal class RedisConfigurationManager
    {
#if NETFRAMEWORK
#else
        public static readonly IConfiguration Configuration = null;
#endif

        static RedisConfigurationManager()
        {
#if NETFRAMEWORK
#else
            try
            {
                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                    .Build();
            }
            catch (Exception e)
            {
                Configuration = null;
            }
#endif
        }

        internal static List<MRedisSetting> GetSettings(string key)
        {
            List<MRedisSetting> res = new List<MRedisSetting>();
#if NETFRAMEWORK
            var tmp = ConfigurationManager.GetSection(key);
            var section = tmp as RedisSettingSection;
            foreach (ConnStrSection conn in section.RedisConnections)
            {
                MRedisSetting item = new MRedisSetting()
                {
                    ConnectionStr = conn.ConnectionStr,
                    DbAlias = conn.DbAlias,
                    DbNumber = conn.DbNumber,
                };
                res.Add(item);
            }
#else
            if (Configuration == null)
            {
                return res;
            }
            object obj = Configuration.GetSection(key).Get<List<MRedisSetting>>();
            if (obj == null)
            {
                return res;
            }
            res = (List<MRedisSetting>)obj;
#endif
            return res;
        }
    }

#if NETFRAMEWORK
    public class RedisSettingSection : ConfigurationSection
    {
        [ConfigurationProperty("RedisConnections", IsRequired = true)]
        [ConfigurationCollection(typeof(ConnStrSection), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "ConnStr")]
        public RedisConnectionsContainer RedisConnections
        {
            get => base["RedisConnections"] as RedisConnectionsContainer;
            set => base["RedisConnections"] = value;
        }
    }

    public class RedisConnectionsContainer : ConfigurationElementCollection
    {
        /// <summary>在派生的类中重写时，创建一个新的 <see cref="T:System.Configuration.ConfigurationElement" />。</summary>
        /// <returns>一个新创建的 <see cref="T:System.Configuration.ConfigurationElement" />。</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnStrSection();
        }

        /// <summary>在派生类中重写时获取指定配置元素的元素键。</summary>
        /// <param name="element">要为其返回键的 <see cref="T:System.Configuration.ConfigurationElement" />。</param>
        /// <returns>一个 <see cref="T:System.Object" />，用作指定 <see cref="T:System.Configuration.ConfigurationElement" /> 的键。</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ConnStrSection).DbNumber;
        }
    }

    public class ConnStrSection : ConfigurationElement
    {
        [ConfigurationProperty("DbNumber", IsRequired = true)]
        public int DbNumber
        {
            get => Convert.ToInt32(base["DbNumber"]);
            set => base["DbNumber"] = value;
        }

        [ConfigurationProperty("DbAlias", IsRequired = true)]
        public string DbAlias { get => base["DbAlias"].ToString(); set => base["DbAlias"] = value; }

        [ConfigurationProperty("ConnectionStr", IsRequired = true)]
        public string ConnectionStr { get => base["ConnectionStr"].ToString(); set => base["ConnectionStr"] = value; }

        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"db={DbNumber}{Environment.NewLine}" +
                   $"dbAlias={DbAlias}{Environment.NewLine}" +
                   $"connStr={ConnectionStr}";
        }
    }
#else
#endif
}
