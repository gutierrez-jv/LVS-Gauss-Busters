using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NCalc;
using LVS_Gauss_Busters.Models;
using System.Linq;

namespace LVS_Gauss_Busters
{
    public static class EquationSolver
    {
        private static string PreprocessEquation(string equation)
        {
            equation = Regex.Replace(equation, @"(\d)([a-zA-Z])", "$1*$2");
            equation = Regex.Replace(equation, @"√\(?([a-zA-Z0-9\.\+\-\*/\^]+)\)?", "Sqrt($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"sqrt\s*\(\s*([^)]+)\s*\)", "Sqrt($1)", RegexOptions.IgnoreCase);

            // ✅ Fix log(x) issue: interpret as natural log
            equation = Regex.Replace(equation, @"(?<![a-zA-Z])log\s*\(\s*([^)]+)\s*\)", "Ln($1)", RegexOptions.IgnoreCase);

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

            expr.EvaluateFunction += (name, args) =>
            {
                if (name.Equals("ln", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters.Length != 1)
                        throw new ArgumentException("ln() takes exactly one argument");

                    double value = Convert.ToDouble(args.Parameters[0].Evaluate());
                    args.Result = Math.Log(value); // Natural log
                }
            };

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


        public static double[] GaussianElimination(double[,] matrix)
        {
            int n = matrix.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                // Search for maximum in this column
                double maxEl = Math.Abs(matrix[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(matrix[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(matrix[k, i]);
                        maxRow = k;
                    }
                }

                // Swap maximum row with current row (column by column)
                for (int k = i; k < n + 1; k++)
                {
                    double tmp = matrix[maxRow, k];
                    matrix[maxRow, k] = matrix[i, k];
                    matrix[i, k] = tmp;
                }

                // Make all rows below this one 0 in current column
                for (int k = i + 1; k < n; k++)
                {
                    double c = -matrix[k, i] / matrix[i, i];
                    for (int j = i; j < n + 1; j++)
                    {
                        if (i == j)
                        {
                            matrix[k, j] = 0;
                        }
                        else
                        {
                            matrix[k, j] += c * matrix[i, j];
                        }
                    }
                }
            }

            // Solve equation Ax=b for an upper triangular matrix A
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = matrix[i, n] / matrix[i, i];
                for (int k = i - 1; k >= 0; k--)
                {
                    matrix[k, n] -= matrix[k, i] * x[i];
                }
            }

            return x;
        }

        public static double[] GaussianElimination(double[,] matrix, List<string> steps)
        {
            int n = matrix.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                // Search for maximum in this column
                double maxEl = Math.Abs(matrix[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(matrix[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(matrix[k, i]);
                        maxRow = k;
                    }
                }

                // Swap maximum row with current row (column by column)
                for (int k = i; k < n + 1; k++)
                {
                    double tmp = matrix[maxRow, k];
                    matrix[maxRow, k] = matrix[i, k];
                    matrix[i, k] = tmp;
                }
                steps.Add($"Swapped row {i + 1} with row {maxRow + 1}");

                // Make all rows below this one 0 in current column
                for (int k = i + 1; k < n; k++)
                {
                    double c = -matrix[k, i] / matrix[i, i];
                    for (int j = i; j < n + 1; j++)
                    {
                        if (i == j)
                        {
                            matrix[k, j] = 0;
                        }
                        else
                        {
                            matrix[k, j] += c * matrix[i, j];
                        }
                    }
                    steps.Add($"Row {k + 1} updated with multiplier {c:F5}");
                }
            }

            // Solve equation Ax=b for an upper triangular matrix A
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = matrix[i, n] / matrix[i, i];
                for (int k = i - 1; k >= 0; k--)
                {
                    matrix[k, n] -= matrix[k, i] * x[i];
                }
                steps.Add($"Solved for x[{i + 1}] = {x[i]:F5}");
            }

            return x;
        }

        public static double[] GaussJordanElimination(double[,] matrix)
        {
            int n = matrix.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                // Make the diagonal contain all ones
                double diag = matrix[i, i];
                for (int j = 0; j < n + 1; j++)
                {
                    matrix[i, j] /= diag;
                }

                // Make the other elements in column i zero
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = matrix[k, i];
                        for (int j = 0; j < n + 1; j++)
                        {
                            matrix[k, j] -= factor * matrix[i, j];
                        }
                    }
                }
            }

            // Extract solution
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
            {
                x[i] = matrix[i, n];
            }

            return x;
        }

        public static double[] GaussJordanElimination(double[,] matrix, List<string> steps)
        {
            int n = matrix.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                // Make the diagonal contain all ones
                double diag = matrix[i, i];
                for (int j = 0; j < n + 1; j++)
                {
                    matrix[i, j] /= diag;
                }
                steps.Add($"Normalized row {i + 1}");

                // Make the other elements in column i zero
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = matrix[k, i];
                        for (int j = 0; j < n + 1; j++)
                        {
                            matrix[k, j] -= factor * matrix[i, j];
                        }
                        steps.Add($"Row {k + 1} updated to eliminate column {i + 1}");
                    }
                }
            }

            // Extract solution
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
            {
                x[i] = matrix[i, n];
                steps.Add($"Solved for x[{i + 1}] = {x[i]:F5}");
            }

            return x;
        }

        public static double[] GaussSeidel(double[,] A = null, double[] b = null, List<SystemSolutionResult> systemSteps = null, double[] initialGuess = null, double tolerance = 1e-10, int maxIterations = 1000)
        {
            if (A == null || b == null || systemSteps == null)
            {
                throw new ArgumentNullException("The coefficient matrix (A), constant vector (b), and systemSteps list cannot be null.");
            }

            int n = b.Length;
            double[] x = new double[n];
            if (initialGuess != null)
                Array.Copy(initialGuess, x, n);

            for (int iter = 0; iter < maxIterations; iter++)
            {
                double[] xNew = new double[n];
                Array.Copy(x, xNew, n);

                for (int i = 0; i < n; i++)
                {
                    double sum = b[i];
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                            sum -= A[i, j] * xNew[j];
                    }
                    xNew[i] = sum / A[i, i];
                }

                var errors = new List<double>();
                for (int i = 0; i < n; i++)
                {
                    errors.Add(Math.Abs(xNew[i] - x[i]));
                }

                systemSteps.Add(new SystemSolutionResult
                {
                    Iteration = iter + 1,
                    Values = xNew.ToList(),
                    Errors = errors
                });

                double error = errors.Sum();
                if (error < tolerance)
                    break;

                x = xNew;
            }

            return x;
        }


        private static double Derivative(string equation, double x)
        {
            const double h = 1e-5;
            return (EvaluateFunction(equation, x + h) - EvaluateFunction(equation, x - h)) / (2 * h);
        }
    }
}
