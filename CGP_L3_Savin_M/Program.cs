using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CGP_L3_Savin_M;

namespace CGP_L3_Savin_M
{
    class Program
    {
        static Calculatable processor = new Processor16();

        static double a = -7, b = 7, n = 35;

        static void Main(string[] args)
        {
            var h = (b - a) / n;

            Console.WriteLine("Parameters:");
            Console.WriteLine("a = " + a + ", b = " + b + ", n = " + n + ", h = " + h);
            Console.WriteLine();

            Task.WhenAll(
                Enumerable.Range(0, (int)n + 1).Select(i => processor.Process(a, i, h))
            ).Result.ToList().ForEach((i, r) => Console.WriteLine("[" + i + "]: " + r));
        }
    }
}
