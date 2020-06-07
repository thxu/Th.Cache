using Console = System.Console;

namespace Th.Cache.NetFramework.TestExe
{
    class Program
    {
        static void Main(string[] args)
        {
            //var testRes = new MemoryCacheTest().TestAdd();
            var redisAddTestRes = new RedisCacheTest().AddTest();

            Console.WriteLine("hello");
            Console.ReadKey();
        }
    }
}
