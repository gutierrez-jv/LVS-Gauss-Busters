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
            try
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

                double result = Convert.ToDouble(expr.Evaluate());
                Console.WriteLine($"EvaluateFunction: equation = {equation}, x = {x}, result = {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluating the equation '{equation}' at x = {x}: {ex.Message}");
                throw new Exception($"Error evaluating the equation '{equation}' at x = {x}: {ex.Message}");
            }
        }


        public static double Bisection(string equation, double xl, double xr, List<StepResult> steps)
        {
            const double tolerance = 0.0001;
            const int maxIterations = 1000;
            int iteration = 1;

            // Evaluate function values at xl and xr
            double fxl = EvaluateFunction(equation, xl);
            double fxr = EvaluateFunction(equation, xr);

            // Check if the initial guesses bracket the root
            if (fxl * fxr > 0)
            {
                throw new ArgumentException($"The initial guesses do not bracket the root. Ensure f(xl) and f(xr) have opposite signs. f(xl) = {fxl:F5}, f(xr) = {fxr:F5}");
            }

            double xm = xl;
            double previousXm = xl;

            while ((xr - xl) > tolerance && iteration <= maxIterations)
            {
                previousXm = xm;
                xm = (xl + xr) / 2;
                double fxm = EvaluateFunction(equation, xm);

                // Add the current step to the steps list
                steps.Add(new StepResult
                {
                    Iteration = iteration,
                    X = xm,
                    Fx = fxm,
                    Error = iteration == 1 ? 0 : Math.Abs(xm - previousXm),
                    Xl = xl,
                    Xr = xr,
                    Xm = xm,
                    Fxl = fxl,
                    Fxr = fxr,
                    Fxm = fxm
                });

                Console.WriteLine($"Iter {iteration}: xl = {xl}, xr = {xr}, xm = {xm}, f(xl) = {fxl}, f(xr) = {fxr}, f(xm) = {fxm}");

                // Update the interval based on the sign of f(xm)
                if (fxl * fxm < 0)
                {
                    xr = xm;
                    fxr = fxm;
                }
                else
                {
                    xl = xm;
                    fxl = fxm;
                }

                // Check for convergence
                if (Math.Abs(fxm) < tolerance || Math.Abs(xr - xl) < tolerance)
                    break;

                iteration++;
            }

            if (iteration > maxIterations)
            {
                throw new Exception("Bisection method did not converge within the maximum number of iterations.");
            }

            return xm;
        }



        public static double NewtonRaphson(string equation, double x0, List<StepResult> steps)
        {
            const double tolerance = 0.0001;
            const int maxIterations = 1000;
            int iteration = 1;
            double x = x0;
            double previousX = 0;

            while (iteration <= maxIterations)
            {
                double fx = EvaluateFunction(equation, x);
                double dfx = Derivative(equation, x);

                if (Math.Abs(dfx) < 1e-6)
                    throw new Exception("Derivative too small. Newton-Raphson may fail.");

                previousX = x;
                double xNew = x - fx / dfx;

                steps.Add(new StepResult
                {
                    Iteration = iteration,
                    X = xNew,
                    Fx = fx,
                    Error = Math.Abs(xNew - previousX),
                    Xl = x,
                    Xm = xNew,
                    Fxl = fx,
                    Fxm = dfx,
                    Fxr = 0
                });

                Console.WriteLine($"Iter {iteration}: x = {x}, f(x) = {fx}, f'(x) = {dfx}");

                if (Math.Abs(xNew - x) < tolerance)
                    return xNew;

                x = xNew;
                iteration++;
            }

            throw new Exception("Newton-Raphson method did not converge within the maximum number of iterations.");
        }

        public static double Secant(string equation, double x0, double x1, List<StepResult> steps)
        {
            const double tolerance = 0.0001;
            const int maxIterations = 1000;
            int iteration = 1;
            double x2 = 0;

            while (iteration <= maxIterations)
            {
                double fx0 = EvaluateFunction(equation, x0);
                double fx1 = EvaluateFunction(equation, x1);

                if (Math.Abs(fx1 - fx0) < 1e-6)
                    throw new Exception("Division by zero risk in Secant method.");

                double previousX2 = x2;
                x2 = x1 - fx1 * (x1 - x0) / (fx1 - fx0);

                steps.Add(new StepResult
                {
                    Iteration = iteration,
                    X = x2,
                    Fx = EvaluateFunction(equation, x2),
                    Error = Math.Abs(x2 - previousX2),
                    Xl = x0,
                    Xr = x1,
                    Xm = x2,
                    Fxl = fx0,
                    Fxr = fx1,
                    Fxm = EvaluateFunction(equation, x2)
                });

                Console.WriteLine($"Iter {iteration}: x0 = {x0}, x1 = {x1}, x2 = {x2}, f(x0) = {fx0}, f(x1) = {fx1}");

                if (Math.Abs(x2 - x1) < tolerance)
                    return x2;

                x0 = x1;
                x1 = x2;
                iteration++;
            }

            throw new Exception("Secant method did not converge within the maximum number of iterations.");
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

            steps.Add(FormatMatrix(matrix, "Initial Augmented Matrix"));

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
                steps.Add($"Swapped Row {i + 1} with Row {maxRow + 1}");
                steps.Add(FormatMatrix(matrix, "After Row Swap"));

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
                    steps.Add($"Row {k + 1} = Row {k + 1} + ({c:F3}) * Row {i + 1}");
                    steps.Add(FormatMatrix(matrix, "After Elimination Step"));
                }
            }

            // Normalize the diagonal to 1
            for (int i = 0; i < n; i++)
            {
                double diag = matrix[i, i];
                for (int j = 0; j < n + 1; j++)
                {
                    matrix[i, j] /= diag;
                }
                steps.Add($"Row {i + 1} = Row {i + 1} / {diag:F3}");
            }
            steps.Add(FormatMatrix(matrix, "After Normalizing Diagonal"));

            // Solve equation Ax=b for an upper triangular matrix A
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = matrix[i, n];
                for (int k = i - 1; k >= 0; k--)
                {
                    matrix[k, n] -= matrix[k, i] * x[i];
                }
            }

            steps.Add("Back Substitution:");
            for (int i = 0; i < n; i++)
            {
                steps.Add($"x[{i + 1}] = {x[i]:F4}");
            }

            return x;
        }

        public static double[] GaussJordanElimination(double[,] matrix, List<string> steps)
        {
            int n = matrix.GetLength(0);

            steps.Add(FormatMatrix(matrix, "Initial Augmented Matrix"));

            for (int i = 0; i < n; i++)
            {
                // Normalize the diagonal element to 1
                double diag = matrix[i, i];
                for (int j = 0; j < n + 1; j++)
                {
                    matrix[i, j] /= diag;
                }
                steps.Add($"Row {i + 1} = Row {i + 1} / {diag:F3}");
                steps.Add(FormatMatrix(matrix, "After Normalizing Row"));

                // Eliminate all other elements in column i
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = matrix[k, i];
                        for (int j = 0; j < n + 1; j++)
                        {
                            matrix[k, j] -= factor * matrix[i, j];
                        }
                        steps.Add($"Row {k + 1} = Row {k + 1} - ({factor:F3}) * Row {i + 1}");
                        steps.Add(FormatMatrix(matrix, "After Elimination Step"));
                    }
                }
            }

            // Extract solution
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
            {
                x[i] = matrix[i, n];
            }

            steps.Add("Solution:");
            for (int i = 0; i < n; i++)
            {
                steps.Add($"x[{i + 1}] = {x[i]:F4}");
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
            try
            {
                double derivative = (EvaluateFunction(equation, x + h) - EvaluateFunction(equation, x - h)) / (2 * h);
                Console.WriteLine($"Derivative: equation = {equation}, x = {x}, derivative = {derivative}");
                return derivative;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating derivative of '{equation}' at x = {x}: {ex.Message}");
                throw new Exception($"Error calculating derivative of '{equation}' at x = {x}: {ex.Message}");
            }
        }

        private static string FormatMatrix(double[,] matrix, string title = "")
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var result = new List<string>();

            if (!string.IsNullOrEmpty(title))
            {
                result.Add($"--- {title} ---");
            }

            for (int i = 0; i < rows; i++)
            {
                var row = "| ";
                for (int j = 0; j < cols; j++)
                {
                    row += $"{matrix[i, j],8:F3} ";
                }
                row += "|";
                result.Add(row);
            }

            return string.Join("\n", result);
        }
    }
}
