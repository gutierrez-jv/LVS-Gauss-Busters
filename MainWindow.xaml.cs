using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using LVS_Gauss_Busters.Models;
using ScottPlot;
using ScottPlot.WinUI;
using System.Threading.Tasks;

namespace LVS_Gauss_Busters
{
    public sealed partial class MainWindow : Window
    {
        private int numberOfEquations = 3; // Default value
        private List<TextBox> equationInputFields = new();
        private List<TextBox> initialGuessInputFields = new();
        private StackPanel PointInputPanelField;
        private TextBlock SolutionTextBlockField;
        private WinUIPlot PlotViewField;
        internal ComboBox PointCountSelectorField;

        public WinUIPlot PlotViewControl
        {
            get => PlotViewField;
            private set => PlotViewField = value;
        }
        public MainWindow()
        {
            this.InitializeComponent();
            SolutionTextBlockField = (TextBlock)((FrameworkElement)this.Content).FindName("SolutionTextBlock");
            PointCountSelectorField = (ComboBox)((FrameworkElement)this.Content).FindName("PointCountSelector");
            PointInputPanelField = (StackPanel)((FrameworkElement)this.Content).FindName("PointInputPanel");
            PlotViewControl = (WinUIPlot)((FrameworkElement)this.Content).FindName("PlotView"); // Use PlotViewControl here
            UpdateInputVisibility();
            GenerateEquationInputFields();
            GenerateInitialGuessInputFields();
        }

        private TextBlock NumberOfPointsLabel;

        private void UpdateInputVisibility()
        {
            bool isLinearSolver = MethodSelector.SelectedItem is ComboBoxItem selectedItem &&
                                  (selectedItem.Content?.ToString().Contains("Gaussian") == true ||
                                   selectedItem.Content?.ToString().Contains("Gauss") == true);

            NumberOfEquationsLabel.Visibility = isLinearSolver ? Visibility.Visible : Visibility.Collapsed;
            NumberOfEquationsPanel.Visibility = isLinearSolver ? Visibility.Visible : Visibility.Collapsed;
            EquationsLabel.Visibility = isLinearSolver ? Visibility.Visible : Visibility.Collapsed;
            EquationsInputPanel.Visibility = isLinearSolver ? Visibility.Visible : Visibility.Collapsed;
            InitialGuessesLabel.Visibility = (isLinearSolver && MethodSelector.SelectedItem is ComboBoxItem gsItem && gsItem.Content?.ToString() == "Gauss-Seidel") ? Visibility.Visible : Visibility.Collapsed;
            InitialGuessesInputPanel.Visibility = (isLinearSolver && MethodSelector.SelectedItem is ComboBoxItem gsItem2 && gsItem2.Content?.ToString() == "Gauss-Seidel") ? Visibility.Visible : Visibility.Collapsed;

            SingleEquationPanel.Visibility = !isLinearSolver ? Visibility.Visible : Visibility.Collapsed;
            InitialGuessXaPanel.Visibility = !isLinearSolver ? Visibility.Visible : Visibility.Collapsed;
            InitialGuessXbPanel.Visibility = !isLinearSolver ? Visibility.Visible : Visibility.Collapsed;

            if (!isLinearSolver)
            {
                // Ensure single equation solver inputs are visible based on the selected method
                string? method = ((ComboBoxItem?)MethodSelector.SelectedItem)?.Content?.ToString();
                InitialGuessXbPanel.Visibility = (method == "Bisection" || method == "Secant") ? Visibility.Visible : Visibility.Collapsed;
            }

            // Show regression inputs for regression methods
            bool isRegression = MethodSelector.SelectedItem is ComboBoxItem regressionItem &&
                                (regressionItem.Content?.ToString() == "Linear Regression" ||
                                 regressionItem.Content?.ToString() == "Polynomial Regression");

            RegressionInputPanel.Visibility = isRegression ? Visibility.Visible : Visibility.Collapsed;
            PolynomialDegreeLabel.Visibility = MethodSelector.SelectedItem is ComboBoxItem polyItem &&
                                                polyItem.Content?.ToString() == "Polynomial Regression"
                                                ? Visibility.Visible : Visibility.Collapsed;
            PolynomialDegreeTextBox.Visibility = PolynomialDegreeLabel.Visibility;

            // Ensure only the dropdown for number of points is visible for regression methods
            PointCountSelectorField.Visibility = isRegression ? Visibility.Visible : Visibility.Collapsed;
        }

        private void GenerateEquationInputFields()
        {
            EquationsInputPanel.Children.Clear();
            equationInputFields.Clear();
            for (int i = 0; i < numberOfEquations; i++)
            {
                var equationPanel = new Microsoft.UI.Xaml.Controls.StackPanel { Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal, Spacing = 5 };
                for (int j = 0; j < numberOfEquations; j++)
                {
                    equationPanel.Children.Add(new TextBox
                    {
                        Width = 60,
                        PlaceholderText = $"a{i + 1}{GetVariableLetter(j)}",
                        Background = new SolidColorBrush(Microsoft.UI.Colors.DimGray),
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center // Fully qualify VerticalAlignment
                    });
                }
                equationPanel.Children.Add(new TextBlock
                {
                    Text = "=",
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center // Fully qualify VerticalAlignment
                });
                equationPanel.Children.Add(new TextBox
                {
                    Width = 60,
                    PlaceholderText = $"b{i + 1}",
                    Background = new SolidColorBrush(Microsoft.UI.Colors.DimGray),
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center // Fully qualify VerticalAlignment
                });
                EquationsInputPanel.Children.Add(equationPanel);
                equationInputFields.AddRange(equationPanel.Children.OfType<TextBox>());
            }
        }

        private void GenerateInitialGuessInputFields()
        {
            InitialGuessesInputPanel.Children.Clear();
            initialGuessInputFields.Clear();
            for (int i = 0; i < numberOfEquations; i++)
            {
                InitialGuessesInputPanel.Children.Add(new TextBox
                {
                    Width = 60,
                    PlaceholderText = $"Initial {GetVariableLetter(i)}",
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DimGray),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                });
            }
            initialGuessInputFields = InitialGuessesInputPanel.Children.OfType<TextBox>().ToList();
        }

        private string GetVariableLetter(int index)
        {
            return index < 26 ? ((char)('x' + index)).ToString() : $"v{index + 1}";
        }

        private void NumberOfEquationsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(NumberOfEquationsTextBox.Text, out int n) && n > 0)
            {
                numberOfEquations = n;
                GenerateEquationInputFields();
                GenerateInitialGuessInputFields();
            }
            else
            {
                // Optionally show an error message to the user
            }
        }

        private void MethodSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateInputVisibility();
        }

        private async void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            string? method = ((ComboBoxItem?)MethodSelector.SelectedItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(method))
            {
                FinalRootText.Text = "Please select a method.";
                ResultListView.ItemsSource = null;
                return;
            }

            if (method == "Bisection")
            {
                if (string.IsNullOrWhiteSpace(EquationInput.Text))
                {
                    FinalRootText.Text = "Please enter an equation.";
                    ResultListView.ItemsSource = null;
                    return;
                }
                if (!double.TryParse(X0Input.Text, out double xa))
                {
                    FinalRootText.Text = "Invalid input for initial guess (xa).";
                    ResultListView.ItemsSource = null;
                    return;
                }
                if (!double.TryParse(X1Input.Text, out double xb))
                {
                    FinalRootText.Text = "Invalid input for second guess (xb).";
                    ResultListView.ItemsSource = null;
                    return;
                }

                List<StepResult> steps = new();
                try
                {
                    double root = EquationSolver.Bisection(EquationInput.Text, xa, xb, steps);
                    ResultListView.ItemsSource = steps.Select(step =>
                        $"Iter: {step.Iteration}, xl: {step.Xl:F4}, xr: {step.Xr:F4}, xm: {step.Xm:F4}, f(xm): {step.Fxm:F4}, Error: {step.Error:F4}"
                    ).ToList();
                    FinalRootText.Text = $"Approximate Root: {root:F5}";
                }
                catch (ArgumentException ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                    ResultListView.ItemsSource = null;
                }
                catch (Exception ex) // Catch other potential errors
                {
                    FinalRootText.Text = $"Error during Bisection: {ex.Message}";
                    ResultListView.ItemsSource = null;
                }
                return;
            }
            // NEWTON-RAPHSON METHOD
            else if (method == "Newton-Raphson")
            {
                if (string.IsNullOrWhiteSpace(EquationInput.Text))
                {
                    FinalRootText.Text = "Please enter an equation.";
                    ResultListView.ItemsSource = null;
                    return;
                }
                if (!double.TryParse(X0Input.Text, out double x0))
                {
                    FinalRootText.Text = "Invalid input for initial guess (x0).";
                    ResultListView.ItemsSource = null;
                    return;
                }

                List<StepResult> steps = new();
                try
                {
                    double root = EquationSolver.NewtonRaphson(EquationInput.Text, x0, steps);
                    ResultListView.ItemsSource = steps.Select(step =>
                        $"Iter: {step.Iteration}, x: {step.X:F4}, f(x): {step.Fx:F4}, f'(x): {step.Fxm:F4}, Error: {step.Error:F4}"
                    ).ToList();
                    FinalRootText.Text = $"Approximate Root: {root:F5}";
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error during Newton-Raphson: {ex.Message}";
                    ResultListView.ItemsSource = null;
                }
                return;
            }
            // SECANT METHOD
            else if (method == "Secant")
            {
                if (string.IsNullOrWhiteSpace(EquationInput.Text))
                {
                    FinalRootText.Text = "Please enter an equation.";
                    ResultListView.ItemsSource = null;
                    return;
                }
                if (!double.TryParse(X0Input.Text, out double x0))
                {
                    FinalRootText.Text = "Invalid input for first guess (x0).";
                    ResultListView.ItemsSource = null;
                    return;
                }
                if (!double.TryParse(X1Input.Text, out double x1))
                {
                    FinalRootText.Text = "Invalid input for second guess (x1).";
                    ResultListView.ItemsSource = null;
                    return;
                }

                List<StepResult> steps = new();
                try
                {
                    double root = EquationSolver.Secant(EquationInput.Text, x0, x1, steps);
                    ResultListView.ItemsSource = steps.Select(step =>
                        $"Iter: {step.Iteration}, x0: {step.Xl:F4}, x1: {step.Xr:F4}, x2: {step.Xm:F4}, f(x2): {step.Fx:F4}, Error: {step.Error:F4}"
                    ).ToList();
                    FinalRootText.Text = $"Approximate Root: {root:F5}";
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error during Secant: {ex.Message}";
                    ResultListView.ItemsSource = null;
                }
                return;
            }
            // LINEAR SYSTEM METHODS
            if (method.Contains("Gaussian") || method.Contains("Gauss"))
            {
                try
                {
                    int n = numberOfEquations;
                    double[,] augmentedMatrix = new double[n, n + 1];

                    // Read coefficients from input fields
                    for (int i = 0; i < n; i++)
                    {
                        var equationPanel = (StackPanel)EquationsInputPanel.Children[i];
                        for (int j = 0; j < n; j++)
                        {
                            if (!double.TryParse(((TextBox)equationPanel.Children[j]).Text, out double coeff))
                            {
                                FinalRootText.Text = $"Invalid coefficient in equation {i + 1} for {GetVariableLetter(j)}.";
                                ResultListView.ItemsSource = null;
                                return;
                            }
                            augmentedMatrix[i, j] = coeff;
                        }
                        if (!double.TryParse(((TextBox)equationPanel.Children[n + (n > 0 ? 1 : 0)]).Text, out double constant)) // Adjust index based on variable labels
                        {
                            FinalRootText.Text = $"Invalid constant term in equation {i + 1}.";
                            ResultListView.ItemsSource = null;
                            return;
                        }
                        augmentedMatrix[i, n] = constant;
                    }

                    List<string> stepsDisplay = new();
                    double[]? result = null;

                    if (method == "Gaussian Elimination")
                    {
                        double[,] matrixCopy = (double[,])augmentedMatrix.Clone();
                        result = EquationSolver.GaussianElimination(matrixCopy, stepsDisplay);
                        ResultListView.ItemsSource = stepsDisplay;
                    }
                    else if (method == "Gauss-Jordan")
                    {
                        double[,] matrixCopy = (double[,])augmentedMatrix.Clone();
                        result = EquationSolver.GaussJordanElimination(matrixCopy, stepsDisplay);
                        ResultListView.ItemsSource = stepsDisplay;
                    }
                    else if (method == "Gauss-Seidel")
                    {
                        double[,] aMatrix = new double[n, n];
                        double[] b = new double[n];
                        double[] initialGuess = new double[n];
                        List<SystemSolutionResult> systemSteps = new();

                        for (int i = 0; i < n; i++)
                        {
                            var equationPanel = (StackPanel)EquationsInputPanel.Children[i];
                            for (int j = 0; j < n; j++)
                            {
                                if (!double.TryParse(((TextBox)equationPanel.Children[j]).Text, out aMatrix[i, j]))
                                {
                                    FinalRootText.Text = $"Invalid coefficient in equation {i + 1} for {GetVariableLetter(j)}.";
                                    ResultListView.ItemsSource = null;
                                    return;
                                }
                            }
                            if (!double.TryParse(((TextBox)equationPanel.Children[n + (n > 0 ? 1 : 0)]).Text, out b[i]))
                            {
                                FinalRootText.Text = $"Invalid constant term in equation {i + 1}.";
                                ResultListView.ItemsSource = null;
                                return;
                            }
                            if (i < initialGuessInputFields.Count && double.TryParse(initialGuessInputFields[i].Text, out double guess))
                            {
                                initialGuess[i] = guess;
                            }
                            else
                            {
                                initialGuess[i] = 0; // Default initial guess
                            }
                        }

                        result = EquationSolver.GaussSeidel(aMatrix, b, systemSteps, initialGuess);

                        // Full solution output formatting
                        var output = new List<string>
                        {
                            "Solution:",
                            "Initial Guesses:"
                        };
                        output.AddRange(initialGuess.Select((g, i) => $"{GetVariableLetter(i)} = {g:F4}"));
                        output.Add("\nStarting iterations:");

                        foreach (var step in systemSteps)
                        {
                            output.Add($"Iteration {step.Iteration}:");
                            for (int i = 0; i < step.Values.Count; i++)
                            {
                                // Generate the detailed calculation for each variable
                                string calculation = $"{GetVariableLetter(i)} = ({b[i]}";
                                for (int j = 0; j < n; j++)
                                {
                                    if (j != i)
                                    {
                                        calculation += $" - {aMatrix[i, j]} * {step.Values[j]:F4}";
                                    }
                                }
                                calculation += $") / {aMatrix[i, i]} = {step.Values[i]:F5}";
                                output.Add(calculation);
                            }
                            output.Add(""); // Blank line for separation
                        }

                        ResultListView.ItemsSource = output;
                    }

                    if (result != null)
                    {
                        FinalRootText.Text = "Solution: " + string.Join(", ", result.Select((r, i) => $"{GetVariableLetter(i)} = {r:F5}"));
                    }
                }
                catch (FormatException)
                {
                    FinalRootText.Text = "Invalid matrix input format.";
                    ResultListView.ItemsSource = null;
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                    ResultListView.ItemsSource = null;
                }

                return;
            }
            else if (method == "Linear Regression")
            {
                await SolveButton_Click_LinearRegression(sender, e);
            }
            else if (method == "Polynomial Regression")
            {
                try
                {
                    double[] x = InputXValues(); // Implement a method to read X values
                    double[] y = InputYValues(); // Implement a method to read Y values
                    int degree = GetPolynomialDegree(); // Implement a method to get the degree

                    double[] coefficients = EquationSolver.PolynomialRegression(x, y, degree);
                    FinalRootText.Text = "y = " + string.Join(" + ", coefficients.Select((c, i) => $"{c:F4}x^{i}"));
                    ResultListView.ItemsSource = new List<string> { "Polynomial regression performed." }; // Or any relevant steps

                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                }
            }
        }

        private double[] InputXValues()
        {
            if (string.IsNullOrWhiteSpace(XValuesTextBox.Text))
                throw new ArgumentException("X values cannot be empty.");

            return XValuesTextBox.Text
                .Split(',')
                .Select(value => double.Parse(value.Trim()))
                .ToArray();
        }

        private double[] InputYValues()
        {
            if (string.IsNullOrWhiteSpace(YValuesTextBox.Text))
                throw new ArgumentException("Y values cannot be empty.");

            return YValuesTextBox.Text
                .Split(',')
                .Select(value => double.Parse(value.Trim()))
                .ToArray();
        }

        private int GetPolynomialDegree()
        {
            if (string.IsNullOrWhiteSpace(PolynomialDegreeTextBox.Text))
                throw new ArgumentException("Polynomial degree cannot be empty.");

            if (!int.TryParse(PolynomialDegreeTextBox.Text, out int degree) || degree < 0)
                throw new ArgumentException("Polynomial degree must be a non-negative integer.");

            return degree;
        }

        private void PointCountSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PointCountSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                int pointCount = int.Parse(selectedItem.Content.ToString());
                PointInputPanel.Children.Clear();

                for (int i = 1; i <= pointCount; i++)
                {
                    var stackPanel = new StackPanel { Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
                    stackPanel.Children.Add(new TextBlock { Text = $"Point {i}:", Foreground = new SolidColorBrush(Microsoft.UI.Colors.White), Width = 60 });
                    stackPanel.Children.Add(new TextBox { Name = $"X{i}", Width = 50, Margin = new Thickness(5, 0, 5, 0) });
                    stackPanel.Children.Add(new TextBox { Name = $"Y{i}", Width = 50 });
                    PointInputPanel.Children.Add(stackPanel);
                }
            }
        }

        private async Task SolveButton_Click_LinearRegression(object sender, RoutedEventArgs e)
        {
            try
            {
                var xValues = PointInputPanel.Children.OfType<StackPanel>()
                    .Select(sp => double.Parse(((TextBox)sp.Children[1]).Text)).ToArray();
                var yValues = PointInputPanel.Children.OfType<StackPanel>()
                    .Select(sp => double.Parse(((TextBox)sp.Children[2]).Text)).ToArray();

                var (slope, intercept) = EquationSolver.LinearRegression(xValues, yValues);

                // Construct the solution text
                var solutionBuilder = new System.Text.StringBuilder();
                solutionBuilder.AppendLine("=== Construct Table of Values ===");
                solutionBuilder.AppendLine($"{"x",10} {"y",10} {"x*y",10} {"x^2",10}");
                double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

                for (int i = 0; i < xValues.Length; i++)
                {
                    double x = xValues[i];
                    double y = yValues[i];
                    double xy = x * y;
                    double x2 = x * x;

                    sumX += x;
                    sumY += y;
                    sumXY += xy;
                    sumX2 += x2;

                    solutionBuilder.AppendLine($"{x,10:F4} {y,10:F4} {xy,10:F4} {x2,10:F4}");
                }

                solutionBuilder.AppendLine();
                solutionBuilder.AppendLine("=== Calculate Summations ===");
                solutionBuilder.AppendLine($"?x = {sumX:F4}, ?y = {sumY:F4}, ?xy = {sumXY:F4}, ?x² = {sumX2:F4}");
                solutionBuilder.AppendLine();
                solutionBuilder.AppendLine("=== Compute the Slope (m) and Intercept (b) ===");
                solutionBuilder.AppendLine($"m = (n?xy - ?x?y) / (n?x² - (?x)²)");
                solutionBuilder.AppendLine($"m = ({xValues.Length} * {sumXY:F4} - {sumX:F4} * {sumY:F4}) / ({xValues.Length} * {sumX2:F4} - {sumX:F4}²) = {slope:F4}");
                solutionBuilder.AppendLine($"b = (?y - m?x) / n");
                solutionBuilder.AppendLine($"b = ({sumY:F4} - {slope:F4} * {sumX:F4}) / {xValues.Length} = {intercept:F4}");
                solutionBuilder.AppendLine();
                solutionBuilder.AppendLine($"Final Linear Regression Equation: y = {slope:F4}x + {intercept:F4}");

                // Display the solution
                SolutionTextBlock.Text = solutionBuilder.ToString();

                // Plot the data
                var plt = PlotView.Plot;
                plt.Clear();
                var scatter = plt.Add.Scatter(xValues, yValues);
                scatter.Label = "Points";
                scatter.Color = ScottPlot.Color.FromSDColor(System.Drawing.Color.Red);

                var line = plt.Add.Function(x => slope * x + intercept);
                line.LineColor = ScottPlot.Color.FromSDColor(System.Drawing.Color.Blue);
                line.Label = "Regression Line";

                plt.Legend.IsVisible = true;
                PlotView.Refresh();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot // Ensure the dialog is tied to the current window
                };
                await errorDialog.ShowAsync(); // Await is now valid because the method is async
            }
        }
    }
}