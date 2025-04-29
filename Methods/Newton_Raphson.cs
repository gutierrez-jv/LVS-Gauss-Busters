using LVS.Methods;
using NCalc; // You’ll install this package
using System;
using System.Linq.Expressions;

namespace LVS.Methods
{
    public class NewtonRaphson : INumericalMethod
    {
        public string Solve(string expression, double x0, double x1 = 0)
        {
            double x = x0, prevX;
            double error = 100;
            int count = 0;
            string result = "";

            while (error > 0.0001 && count < 100)
            {
                var fx = Evaluate(expression, x);
                var dfx = Derivative(expression, x);

                if (Math.Abs(dfx) < 1e-6)
                    return "Derivative too small. Cannot proceed.";

                prevX = x;
                x = x - (fx / dfx);
                error = Math.Abs((x - prevX) / x) * 100;

                result += $"Iter {count + 1}: x = {x:F5}, Error = {error:F5}%\n";
                count++;
            }

            result += $"Approximate root: {x:F5}";
            return result;
        }

        private double Evaluate(string expr, double x)
        {
            var e = new Expression(expr);
            e.Parameters["x"] = x;
            return Convert.ToDouble(e.Evaluate());
        }

        private double Derivative(string expr, double x)
        {
            double h = 1e-5;
            return (Evaluate(expr, x + h) - Evaluate(expr, x - h)) / (2 * h);
        }
    }
}
