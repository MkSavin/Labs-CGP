using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CGP_L3_Savin_M
{
    static class TaskProcessor
    {
        public static async Task<double> Process(this Calculatable c, double a, double i, double h)
        {
            var param = c.Param(a, i, h);

            return
                await Task.Run(() => c.First(param)).ConfigureAwait(false) -
                await Task.Run(() => c.Second(param)).ConfigureAwait(false) -
                await Task.Run(() => c.Third(param)).ConfigureAwait(false) +
                await Task.Run(() => c.Fourth(param)).ConfigureAwait(false);
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
