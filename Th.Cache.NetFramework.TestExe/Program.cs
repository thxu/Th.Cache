using Console = System.Console;

namespace Th.Cache.NetFramework.TestExe
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
