using System;
using System.Threading;
using Th.Cache.MemoryCache;

namespace Th.Cache.NetFramework.TestExe
{
    public class MemoryCacheTest
    {
        internal int Add(int a)
        {
            var key = $"addCache";
            var val = MemoryCacheHelper.Get<int>(key);
            if (val > 0)
            {
                return val;
            }

            a++;
            MemoryCacheHelper.Set(key, a, DateTime.Now.AddSeconds(3));
            return a;
        }

        public bool TestAdd()
        {
            int a = 0;
            // 初次执行函数，返回1
            var addRes = Add(a);
            if (addRes != 1)
            {
                return false;
            }

            // 检查返回值是否已经被缓存，若已被缓存则应该返回1
            a = 100;
            addRes = Add(a);
            if (addRes != 1)
            {
                return false;
            }

            Thread.Sleep(3000);

            // 等待缓存失效后调用函数，此时应该返回101
            a = 100;
            addRes = Add(a);
            if (addRes != 101)
            {
                return false;
            }
            return true;
        }
    }
}
