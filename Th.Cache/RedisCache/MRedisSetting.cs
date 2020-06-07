namespace Th.Cache.RedisCache
{
    /// <summary>
    /// redis配置类
    /// </summary>
    internal class MRedisSetting
    {
        /// <summary>
        /// Db编号
        /// </summary>
        public int DbNumber { get; set; }

        /// <summary>
        /// Db别名，方便记忆，使用的时候传别名或者传db编号都可以
        /// </summary>
        public string DbAlias { get; set; }

        /// <summary>
        /// 连接字符串,格式：127.0.0.1:6379,password=123,defaultDatabase=13,prefix=key前辍
        /// 更多设置参考：https://github.com/2881099/csredis
        /// </summary>
        public string ConnectionStr { get; set; }
    }
}
