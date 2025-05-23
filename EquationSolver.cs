﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NCalc;
using LVS_Gauss_Busters.Models;
using System.Linq;
using System.Text; // Add this namespace at the top of the file


namespace LVS_Gauss_Busters
{
    public static class EquationSolver
    {
        private static string PreprocessEquation(string equation)
        {
            equation = Regex.Replace(equation, @"(\d)([a-zA-Z])", "$1*$2");
            equation = Regex.Replace(equation, @"([a-zA-Z])(\d)", "$1*$2");
            equation = Regex.Replace(equation, @"√\(?([a-zA-Z0-9\.\+\-\*/\^]+)\)?", "Sqrt($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"sqrt\s*\(\s*([^)]+)\s*\)", "Sqrt($1)", RegexOptions.IgnoreCase);
            // Replace both log(x) and ln(x) with ln(x) for consistency
            equation = Regex.Replace(equation, @"(?<![a-zA-Z])log\s*\(\s*([^)]+)\s*\)", "ln($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"(?<![a-zA-Z])ln\s*\(\s*([^)]+)\s*\)", "ln($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"(?<![a-zA-Z])e\s*\^\s*\(?([a-zA-Z0-9\.\+\-\*/\^]+)\)?", "Exp($1)", RegexOptions.IgnoreCase);
            equation = Regex.Replace(equation, @"([a-zA-Z0-9\)\.]+)\s*\^\s*\(?([\-0-9\.\/]+)\)?", "Pow($1,$2)");
            equation = Regex.Replace(equation, @"(?<![0-9a-zA-Z\)])(\d+)\s*/\s*(\d+)", match =>
            {
                double numerator = double.Parse(match.Groups[1].Value);
                double denominator = double.Parse(match.Groups[2].Value);
                return (numerator / denominator).ToString();
            });
            equation = Regex.Replace(equation, @"([a-zA-Z0-9\.\)\(]+)\s*\^\s*([\-]?[0-9\.]+)", "Pow($1,$2)");
            return equation;
        }


        public static double EvaluateFunction(string equation, double x, double y = double.NaN)
        {
            try
            {
                string processed = PreprocessEquation(equation);
                var expr = new Expression(processed);
                expr.Parameters["x"] = x;
                expr.Parameters["y"] = double.IsNaN(y) ? 0 : y;

                expr.EvaluateFunction += (name, args) =>
                {
                    if (name.Equals("ln", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Parameters.Length != 1)
                            throw new ArgumentException("ln() takes exactly one argument");
                        double value = Convert.ToDouble(args.Parameters[0].Evaluate());
                        if (value <= 0)
                            throw new ArgumentException("ln(x) is undefined for x <= 0");
                        args.Result = Math.Log(value);
                    }
                    else if (name.Equals("log", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Parameters.Length != 1)
                            throw new ArgumentException("log() takes exactly one argument");
                        double value = Convert.ToDouble(args.Parameters[0].Evaluate());
                        if (value <= 0)
                            throw new ArgumentException("log(x) is undefined for x <= 0");
                        args.Result = Math.Log10(value); // Base-10 log
                    }
                };


                object result = expr.Evaluate();
                if (result != null)
                {
                    return Convert.ToDouble(result);
                }
                else
                {
                    throw new Exception("Evaluation returned null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluating the equation '{equation}' at x = {x}, y = {y}: {ex.Message}");
                throw;
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


        public static double[] GaussianElimination(double[,] matrix, double[] rhs)
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

        public static (double Slope, double Intercept) LinearRegression(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Input arrays must have the same length.");

            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }

        public static double[] PolynomialRegression(double[] x, double[] y, int degree, List<string> steps)
        {
            if (x == null || y == null || x.Length != y.Length || x.Length <= degree)
            {
                throw new ArgumentException($"At least {degree + 1} data points are required for a polynomial of degree {degree}.");
            }

            int n = x.Length;
            double[,] vandermondeMatrix = new double[n, degree + 1];
            double[,] transposeMatrix = new double[degree + 1, n];
            double[,] ataMatrix = new double[degree + 1, degree + 1];
            double[] atyVector = new double[degree + 1];

            // Step 1: Construct the Vandermonde Matrix A
            steps.Add($"Polynomial Regression (Degree {degree})");
            steps.Add("------------------------------------------------------");
            steps.Add("Construct the Vandermonde Matrix A");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= degree; j++)
                {
                    vandermondeMatrix[i, j] = Math.Pow(x[i], j);
                }
            }
            steps.Add($"A =\n{FormatMatrix(vandermondeMatrix)}");

            // Step 2: Calculate A^T (Transpose of A)
            for (int i = 0; i <= degree; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    transposeMatrix[i, j] = vandermondeMatrix[j, i];
                }
            }

            // Step 3: Calculate A^T * A
            for (int i = 0; i <= degree; i++)
            {
                for (int j = 0; j <= degree; j++)
                {
                    ataMatrix[i, j] = 0;
                    for (int k = 0; k < n; k++)
                    {
                        ataMatrix[i, j] += transposeMatrix[i, k] * vandermondeMatrix[k, j];
                    }
                }
            }
            steps.Add("------------------------------------------------------");
            steps.Add("Calculate A^T * A (A Transpose times A)");
            steps.Add($"A^T * A =\n{FormatMatrix(ataMatrix)}");

            // Step 4: Calculate A^T * y
            for (int i = 0; i <= degree; i++)
            {
                atyVector[i] = 0;
                for (int j = 0; j < n; j++)
                {
                    atyVector[i] += transposeMatrix[i, j] * y[j];
                }
            }
            steps.Add("------------------------------------------------------");
            steps.Add("Calculate A^T * y (A Transpose times y)");
            steps.Add($"A^T * y = [{string.Join(", ", atyVector.Select(v => v.ToString("F4")))}]");

            // Step 5: Solve for coefficients using Gaussian Elimination
            double[,] augmentedMatrix = new double[degree + 1, degree + 2];
            for (int i = 0; i <= degree; i++)
            {
                for (int j = 0; j <= degree; j++)
                {
                    augmentedMatrix[i, j] = ataMatrix[i, j];
                }
                augmentedMatrix[i, degree + 1] = atyVector[i];
            }

            steps.Add("------------------------------------------------------");
            steps.Add("Solve for Coefficients using Gaussian Elimination");
            double[] coefficients = GaussianEliminationForPolynomial(augmentedMatrix, steps);

            steps.Add("Coefficients:");
            for (int i = 0; i < coefficients.Length; i++)
            {
                steps.Add($"c[{i}] = {coefficients[i]:F4}");
            }

            // Step 6: Final Polynomial Regression Equation
            steps.Add("------------------------------------------------------");
            steps.Add("Final Polynomial Regression Equation:");
            string equation = "y = ";
            for (int i = 0; i < coefficients.Length; i++)
            {
                string term;
                if (i == 0)
                {
                    term = $"{coefficients[i]:F4}";
                }
                else if (i == 1)
                {
                    term = $"{coefficients[i]:F4}x";
                }
                else
                {
                    term = $"{coefficients[i]:F4}x^{i}";
                }

                if (i > 0)
                {
                    if (coefficients[i] >= 0)
                    {
                        equation += " + ";
                    }
                    else
                    {
                        equation += " "; // Keep the negative sign
                    }
                }
                equation += term;
            }
            steps.Add(equation);

            // Print all steps
            foreach (var step in steps)
            {
                Console.WriteLine(step);
            }

            return coefficients;
        }


        // Modified Gaussian Elimination for Polynomial Regression (returns coefficients)
        private static double[] GaussianEliminationForPolynomial(double[,] augmentedMatrix, List<string> steps)
        {
            int n = augmentedMatrix.GetLength(0);
            double[] coefficients = new double[n];

            for (int i = 0; i < n; i++)
            {
                // Find pivot row
                int pivotRow = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(augmentedMatrix[j, i]) > Math.Abs(augmentedMatrix[pivotRow, i]))
                    {
                        pivotRow = j;
                    }
                }

                // Swap rows
                if (pivotRow != i)
                {
                    for (int k = 0; k <= n; k++)
                    {
                        (augmentedMatrix[i, k], augmentedMatrix[pivotRow, k]) = (augmentedMatrix[pivotRow, k], augmentedMatrix[i, k]);
                    }
                    steps.Add($"Swapped R{i + 1} with R{pivotRow + 1}");
                }

                // Make the pivot element 1
                double pivot = augmentedMatrix[i, i];

                for (int j = i; j <= n; j++)
                {
                    augmentedMatrix[i, j] /= pivot;
                }
                steps.Add($"R{i + 1} = R{i + 1} / {pivot:F4}");

                // Eliminate other rows
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        double factor = augmentedMatrix[j, i];
                        for (int k = i; k <= n; k++)
                        {
                            augmentedMatrix[j, k] -= factor * augmentedMatrix[i, k];
                        }
                        steps.Add($"R{j + 1} = R{j + 1} - ({factor:F4}) * R{i + 1}");
                    }
                }
            }

            // Extract coefficients (solution)
            for (int i = 0; i < n; i++)
            {
                coefficients[i] = augmentedMatrix[i, n];
            }

            return coefficients;
        }

        public static (string steps, double[] xPoints, double[] yPoints, double result) TrapezoidalRuleWithPlotData(string expression, double a, double b, int n)
        {
            StringBuilder steps = new StringBuilder();
            double h = (b - a) / n;
            steps.AppendLine($"Executing Trapezoidal Rule with n = {n}, step size h = ({b} - {a}) / {n} = {h:F4}");
            steps.AppendLine($"\n--- Step 1: Compute f(x) at the boundaries and intermediate points ---");

            double sum = 0;
            double[] xPoints = new double[n + 1];
            double[] yPoints = new double[n + 1];

            for (int i = 0; i <= n; i++)
            {
                double x = a + i * h;
                double y = EvaluateFunction(expression, x);
                xPoints[i] = x;
                yPoints[i] = y;

                steps.AppendLine($"  f(x[{i}]) = f({x:F4}) = {y:F4}");
                sum += (i == 0 || i == n) ? y : 2 * y;
            }

            steps.AppendLine($"\n--- Step 2: Apply the Trapezoidal Rule formula ---");
            steps.AppendLine($"Integral ≈ (h / 2) * [f(x[0]) + 2f(x[1]) + ... + 2f(x[{n - 1}]) + f(x[{n}])]");
            steps.AppendLine($"         ≈ ({h:F4} / 2) * [{sum:F4}]");
            double result = (h / 2) * sum;
            steps.AppendLine($"         ≈ {result:F4}");

            return (steps.ToString(), xPoints, yPoints, result);
        }




        public static (string steps, double[] xPoints, double[] yPoints, double result) SimpsonsRuleWithPlotData(string expression, double a, double b, int n)
        {
            StringBuilder steps = new StringBuilder();
            if (n % 2 != 0)
            {
                throw new ArgumentException("The number of divisions for Simpson's 1/3 Rule must be an even integer.");
            }

            double h = (b - a) / n;
            steps.AppendLine($"Applying Simpson's 1/3 Rule with n = {n}, h = ({b} - {a}) / {n} = {h:F4}");
            steps.AppendLine($"\n──── Step 1: Evaluate f(x) at the endpoints and intermediate points: ────");
            double sum = 0;
            double[] xValues = new double[n + 1];
            double[] yValues = new double[n + 1];

            for (int i = 0; i <= n; i++)
            {
                double x = a + i * h;
                double y = EvaluateFunction(expression, x);
                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    throw new ArgumentException($"Function evaluation resulted in a non-finite value at x = {x}. Please check your function and bounds.");
                }
                steps.AppendLine($"  f(x_{i}) = f({x:F4}) = {y:F4}");
                if (i == 0 || i == n)
                    sum += y;
                else if (i % 2 == 0)
                    sum += 2 * y;
                else
                    sum += 4 * y;
                xValues[i] = x;
                yValues[i] = y;
            }

            steps.AppendLine($"\n──── Step 2: Apply the Simpson's 1/3 Rule formula: ────");
            steps.AppendLine($"Integral ≈ (h / 3) * [f(x_0) + 4f(x_1) + 2f(x_2) + 4f(x_3) + ... + 4f(x_{n - 1}) + f(x_n)]");
            steps.AppendLine($"         ≈ ({h:F4} / 3) * [{sum:F4}]");
            double result = (h / 3) * sum;
            steps.AppendLine($"         ≈ {result:F4}");

            return (steps.ToString(), xValues, yValues, result);
        }

        public static (string steps, double[] xPoints, double[] yPoints, double lastY) Euler(
            string differentialEquation,
            double initialX,
            double initialY,
            double endX,
            double stepSize
        )
        {
            if (stepSize <= 0)
            {
                throw new ArgumentException("Step size must be positive.");
            }

            StringBuilder steps = new StringBuilder();
            List<double> xPoints = new List<double>();
            List<double> yPoints = new List<double>();

            double currentX = initialX;
            double currentY = initialY;

            xPoints.Add(currentX);
            yPoints.Add(currentY);
            steps.AppendLine($"--- Euler's Method ---");
            steps.AppendLine($"Given y' = {differentialEquation}, y({initialX}) = {initialY}, find y({endX}) with step size h = {stepSize}");

            int iteration = 0;
            while (currentX < endX)
            {
                double derivative = EvaluateFunction(differentialEquation, currentX, currentY);
                double nextY = currentY + stepSize * derivative;
                double nextX = Math.Min(currentX + stepSize, endX);

                steps.AppendLine($"Iteration {iteration}:");
                steps.AppendLine($"  x_{iteration} = {currentX:F4}, y_{iteration} = {currentY:F4}, y'({currentX:F4}, {currentY:F4}) = {derivative:F4}");
                steps.AppendLine($"  y_{iteration + 1} = {currentY:F4} + {stepSize:F4} * {derivative:F4} = {nextY:F4}");

                xPoints.Add(nextX);
                yPoints.Add(nextY);

                currentX = nextX;
                currentY = nextY;
                iteration++;

                if (Math.Abs(currentX - endX) < 1e-9)
                {
                    break;
                }
            }

            steps.AppendLine($"\nApproximate value of y({endX:F4}) ≈ {currentY:F4}");

            return (steps.ToString(), xPoints.ToArray(), yPoints.ToArray(), currentY);
        }
    }
}