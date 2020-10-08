using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CGP_L3_Savin_M
{
    /// <summary>
    /// Variant 16.
    /// 3*Sin(x)/x - ln(5*tg(x)) - exp(7*sqrt(x)) + 0.3*(x^3 + x^2 -1)
    /// </summary>
    class Processor16 : Calculatable
    {
        public double First(double x) => 3 * Math.Sin(x) / x;
        // Здесь используется LN, аргумент 5 * TG(x) может быть отрицательным, выбираем значение по модулю
        public double Second(double x) => Math.Log(Math.Abs(5 * Math.Tan(x)));
        // Здесь используется SQRT, аргумент x может быть отрицательным, выбираем значение по модулю
        public double Third(double x) => Math.Exp(7 * Math.Sqrt(Math.Abs(x)));
        public double Fourth(double x) => 0.3 * (Math.Pow(x, 3) + Math.Pow(x, 2) - 1);

        public double Process(double x) => First(x) - Second(x) - Third(x) + Fourth(x);

        public double Param(double a, double i, double h) => a + h * i;
    }
}
