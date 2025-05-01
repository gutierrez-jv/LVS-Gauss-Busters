using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NCalc;
using LVS_Gauss_Busters.Models; // ✅ Use the correct StepResult

namespace LVS_Gauss_Busters
{
    public static class EquationSolver
    {
        private static string PreprocessEquation(string equation)
        {
            equation = Regex.Replace(equation, @"(\d)([a-zA-Z])", "$1*$2");
            equation = Regex.Replace(equation, @"√\(?([a-zA-Z0-9\.\+\-\*/\^]+)\)?", "Sqrt($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"sqrt\s*\(\s*([^)]+)\s*\)", "Sqrt($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"(?<![a-zA-Z])e\s*\^\s*\(?([a-zA-Z0-9\.\+\-\*/\^]+)\)?", "Exp($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"([a-zA-Z0-9\)\.]+)\s*\^\s*\(?([\-0-9\.\/]+)\)?", "Pow($1,$2)");

            equation = Regex.Replace(equation, @"(?<![0-9a-zA-Z\)])(\d+)\s*/\s*(\d+)", match =>
            {
                double numerator = double.Parse(match.Groups[1].Value);
                double denominator = double.Parse(match.Groups[2].Value);
                return (numerator / denominator).ToString();
            });

            equation = Regex.Replace(
                equation,
                @"([a-zA-Z0-9\.\)\(]+)\s*\^\s*([\-]?[0-9\.]+)",
                "Pow($1,$2)"
            );

            return equation;
        }

        public static double EvaluateFunction(string equation, double x)
        {
            string processed = PreprocessEquation(equation);
            var expr = new Expression(processed);
            expr.Parameters["x"] = x;
            return Convert.ToDouble(expr.Evaluate());
        }

        public static double Bisection(string equation, double xl, double xr, List<StepResult> steps)
        {
            const double tolerance = 0.0001;
            int iteration = 1;
            double xm = 0;

            while ((xr - xl) > tolerance)
            {
                xm = (xl + xr) / 2;
                double fxl = EvaluateFunction(equation, xl);
                double fxr = EvaluateFunction(equation, xr);
                double fxm = EvaluateFunction(equation, xm);

                steps.Add(new StepResult
                {
                    Iteration = iteration,
                    Xl = xl,
                    Xr = xr,
                    Xm = xm,
                    Fxl = fxl,
                    Fxr = fxr,
                    Fxm = fxm
                });

                if (fxl * fxm < 0)
                    xr = xm;
                else
                    xl = xm;

                iteration++;
            }

            return xm;
        }

        public static double NewtonRaphson(string equation, double x0, List<StepResult> steps)
        {
            const double tolerance = 0.0001;
            int iteration = 1;
            double x = x0;

            while (true)
            {
                double fx = EvaluateFunction(equation, x);
                double dfx = Derivative(equation, x);

                if (Math.Abs(dfx) < 1e-6)
                    throw new Exception("Derivative too small. Newton-Raphson may fail.");

                double xNew = x - fx / dfx;

                steps.Add(new StepResult
                {
                    Iteration = iteration,
                    Xl = x,
                    Xm = xNew,
                    Fxl = fx,
                    Fxm = dfx,
                    Fxr = 0 // Optional
                });

                if (Math.Abs(xNew - x) < tolerance)
                    break;

                x = xNew;
                iteration++;
            }

            return x;
        }

        public static double Secant(string equation, double x0, double x1, List<StepResult> steps)
        {
            const double tolerance = 0.0001;
            int iteration = 1;
            double x2 = 0;

            while (true)
            {
                double fx0 = EvaluateFunction(equation, x0);
                double fx1 = EvaluateFunction(equation, x1);

                if (Math.Abs(fx1 - fx0) < 1e-6)
                    throw new Exception("Division by zero risk in Secant method.");

                x2 = x1 - fx1 * (x1 - x0) / (fx1 - fx0);

                steps.Add(new StepResult
                {
                    Iteration = iteration,
                    Xl = x0,
                    Xr = x1,
                    Xm = x2,
                    Fxl = fx0,
                    Fxr = fx1,
                    Fxm = EvaluateFunction(equation, x2)
                });

                if (Math.Abs(x2 - x1) < tolerance)
                    break;

                x0 = x1;
                x1 = x2;
                iteration++;
            }

            return x2;
        }

        private static double Derivative(string equation, double x)
        {
            const double h = 1e-5;
            return (EvaluateFunction(equation, x + h) - EvaluateFunction(equation, x - h)) / (2 * h);
        }
    }
}