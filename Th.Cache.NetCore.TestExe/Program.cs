using System;

namespace Th.Cache.NetCore.TestExe
{
    class Program
    {
        static void Main(string[] args)
        {
            var testRes = new MemoryCacheTest().TestAdd();

            Console.WriteLine("hello");
            Console.ReadKey();
        }
    }
}
