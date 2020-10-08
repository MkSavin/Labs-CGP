using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CGP_L3_Savin_M
{
    static class TaskProcessor
    {
        public static double Process(this Calculatable c, double a, double i, double h)
        {
            return c.Process(c.Param(a, i, h));
        }

        public static void ForEach<T>(this IEnumerable<T> sequence, Action<int, T> action)
        {
            int i = 0;
            foreach (T item in sequence)
            {
                action(i, item);
                i++;
            }
        }
    }
}
