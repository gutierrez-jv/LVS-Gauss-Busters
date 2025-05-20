using LVS_Gauss_Busters.Helpers;
using LVS_Gauss_Busters.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScottPlot;
using ScottPlot.WinUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Radios;
using Windows.Media.Playback;
using Windows.Storage;

namespace LVS_Gauss_Busters
{
    public sealed partial class MainWindow : Window
    {
        private int numberOfEquations = 3; // Default value
        private List<TextBox> equationInputFields = new();
        private List<TextBox> initialGuessInputFields = new();
        private StackPanel PointInputPanelField;
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
            this.Activated += MainWindow_Loaded;
            PointCountSelectorField = (ComboBox)((FrameworkElement)this.Content).FindName("PointCountSelector");
            PointInputPanelField = (StackPanel)((FrameworkElement)this.Content).FindName("PointInputPanel");
            PlotViewControl = (WinUIPlot)((FrameworkElement)this.Content).FindName("PlotView"); // Use PlotViewControl here
            GenerateEquationInputFields();
            GenerateInitialGuessInputFields();
        }

        private void MethodSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMethod = (MethodSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Clear the solution steps and final root text
            ResultListView.ItemsSource = null; // Clear the solution steps
            ResultListView.Items.Clear();
            FinalRootText.Text = ""; // Reset the final root text
            PlotView.Plot.Clear(); // Clear any previous plot
            PlotView.Visibility = Visibility.Collapsed; // Hide the plot

            // Reset UI visibility for all panels
            SingleEquationPanel.Visibility = Visibility.Collapsed;
            InitialGuessXaPanel.Visibility = Visibility.Collapsed;
            InitialGuessXbPanel.Visibility = Visibility.Collapsed;
            NumberOfEquationsPanel.Visibility = Visibility.Collapsed;
            InitialGuessesInputPanel.Visibility = Visibility.Collapsed;
            InitialGuessesLabel.Visibility = Visibility.Collapsed;
            EquationsLabel.Visibility = Visibility.Collapsed;
            EquationsInputPanel.Visibility = Visibility.Collapsed;
            RegressionInputPanel.Visibility = Visibility.Collapsed;
            PolynomialDegreeLabel.Visibility = Visibility.Collapsed;
            PolynomialDegreeTextBox.Visibility = Visibility.Collapsed;
            LinearRegressionData.Visibility = Visibility.Collapsed;
            PointCountSelector.Visibility = Visibility.Collapsed;
            PointInputPanel.Visibility = Visibility.Collapsed;
            PlotStack.Visibility = Visibility.Collapsed;
            IntegrationInputPanel.Visibility = Visibility.Collapsed;
            EulerInputPanel.Visibility = Visibility.Collapsed;

            // Update UI based on the selected method
            if (selectedMethod == "Bisection" || selectedMethod == "Secant")
            {
                SingleEquationPanel.Visibility = Visibility.Visible;
                InitialGuessXaPanel.Visibility = Visibility.Visible;
                InitialGuessXbPanel.Visibility = Visibility.Visible;
            }
            else if (selectedMethod == "Newton-Raphson")
            {
                SingleEquationPanel.Visibility = Visibility.Visible;
                InitialGuessXaPanel.Visibility = Visibility.Visible;
            }
            else if (selectedMethod == "Gaussian Elimination" || selectedMethod == "Gauss-Jordan" || selectedMethod == "Gauss-Seidel")
            {
                NumberOfEquationsPanel.Visibility = Visibility.Visible;
                EquationsLabel.Visibility = Visibility.Visible;
                EquationsInputPanel.Visibility = Visibility.Visible;
                if (selectedMethod == "Gauss-Seidel")
                {
                    InitialGuessesInputPanel.Visibility = Visibility.Visible;
                    InitialGuessesLabel.Visibility = Visibility.Visible;
                }
            }
            else if (selectedMethod == "Linear Regression")
            {
                LinearRegressionData.Visibility = Visibility.Visible;
                PointCountSelector.Visibility = Visibility.Visible;
                PointInputPanel.Visibility = Visibility.Visible;
                PlotStack.Visibility = Visibility.Visible;
            }
            else if (selectedMethod == "Polynomial Regression")
            {
                LinearRegressionData.Visibility = Visibility.Visible;
                PointCountSelector.Visibility = Visibility.Visible;
                PointInputPanel.Visibility = Visibility.Visible;
                PolynomialDegreeLabel.Visibility = Visibility.Visible;
                PolynomialDegreeTextBox.Visibility = Visibility.Visible;
                RegressionInputPanel.Visibility = Visibility.Visible;
                PlotStack.Visibility = Visibility.Visible;
            }
            else if (selectedMethod == "Trapezoidal" || selectedMethod == "Simpson's 1/3")
            {
                IntegrationInputPanel.Visibility = Visibility.Visible;
                PlotStack.Visibility = Visibility.Visible;
            }

            else if (selectedMethod == "Euler")
            {
                EulerInputPanel.Visibility = Visibility.Visible;
                PlotStack.Visibility = Visibility.Visible;
            }
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

        private async void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            await AudioHelper.PlaySoundEffect("Assets/Audio/bust_it.mp3");
            string? method = ((ComboBoxItem?)MethodSelector.SelectedItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(method))
            {
                FinalRootText.Text = "Please select a method.";
                ResultListView.ItemsSource = null;
                return;
            }

            await AudioHelper.PlaySoundEffect("Assets/Audio/bust_it.mp3");

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
                    FinalRootText.Text = $"Approximate Root: {root:F4}";
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
                    FinalRootText.Text = $"Approximate Root: {root:F4}";
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
                    FinalRootText.Text = $"Approximate Root: {root:F4}";
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
                                calculation += $") / {aMatrix[i, i]} = {step.Values[i]:F4}";
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
                    string[] solutionLines = solutionBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    FinalRootText.Text = $"y = {slope:F4}x + {intercept:F4}";

                    // Clear previous items and update ResultListView
                    ResultListView.Items.Clear();
                    foreach (var solutionLine in solutionLines)
                    {
                        if (!string.IsNullOrWhiteSpace(solutionLine))
                            ResultListView.Items.Add(solutionLine);
                    }

                    // Plot the data
                    var plt = PlotView.Plot;
                    plt.Clear();
                    PlotView.Plot.FigureBackground.Color = Color.FromHex("#1a1a1a");
                    PlotView.Plot.DataBackground.Color = Color.FromHex("#1a1a1a");
                    PlotView.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
                    PlotView.Plot.Grid.MajorLineColor = Color.FromHex("#3a3a3a");
                    PlotView.Plot.Legend.BackgroundColor = Color.FromHex("#3a3a3a");
                    PlotView.Plot.Legend.FontColor = Color.FromHex("#e10600");
                    PlotView.Plot.Legend.OutlineColor = Color.FromHex("#e10600");
                    var scatter = plt.Add.Scatter(xValues, yValues);
                    scatter.Label = "Points";
                    scatter.Color = ScottPlot.Color.FromSDColor(System.Drawing.Color.White);

                    var line = plt.Add.Function(x => slope * x + intercept);
                    line.LineColor = ScottPlot.Color.FromHex("#e10600");
                    line.Label = "Regression Line";

                    plt.Legend.IsVisible = true;
                    PlotView.Visibility = Visibility.Visible;
                    PlotView.Refresh();
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                    ResultListView.ItemsSource = new List<string> { $"Error: {ex.Message}" };
                    PlotView.Plot.Clear(); // Clear any previous plot
                    PlotView.Visibility = Visibility.Collapsed; // Hide plot on error
                }
            }

            else if (method == "Polynomial Regression")
            {
                try
                {
                    List<double> xValues = PointInputPanel.Children.OfType<StackPanel>()
                        .Select(sp => double.Parse(((TextBox)sp.Children[1]).Text)).ToList();
                    List<double> yValues = PointInputPanel.Children.OfType<StackPanel>()
                        .Select(sp => double.Parse(((TextBox)sp.Children[2]).Text)).ToList();

                    int degree = GetPolynomialDegree();

                    List<string> steps = new List<string>();
                    double[] coefficients = EquationSolver.PolynomialRegression(xValues.ToArray(), yValues.ToArray(), degree, steps);

                    // Format the polynomial equation for FinalRootText
                    FinalRootText.Text = "y = ";
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
                                FinalRootText.Text += " + ";
                            }
                            else
                            {
                                FinalRootText.Text += " ";
                            }
                        }
                        FinalRootText.Text += term;
                    }

                    // Format the coefficients for ResultListView
                    var coefficientList = new List<string>();
                    coefficientList.Add($"Polynomial Degree: {degree}");
                    for (int i = 0; i < coefficients.Length; i++)
                    {
                        coefficientList.Add($"Coefficient of x^{i}: {coefficients[i]:F4}");
                    }
                    ResultListView.ItemsSource = steps;

                    // Call the plotting function
                    PlotPolynomialRegression(xValues.ToArray(), yValues.ToArray(), coefficients);
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                    ResultListView.ItemsSource = new List<string> { $"Error: {ex.Message}" };
                    PlotView.Plot.Clear(); // Clear any previous plot
                    PlotView.Visibility = Visibility.Collapsed; // Hide plot on error
                }
            }

            else if (method == "Trapezoidal")
            {
                string function = IntegrationFunctionInput.Text;
                if (!double.TryParse(IntegrationLowerBoundInput.Text, out double a) ||
                    !double.TryParse(IntegrationUpperBoundInput.Text, out double b) ||
                    !int.TryParse(IntegrationDivisionsInput.Text, out int n))
                {
                    FinalRootText.Text = "Invalid input for Trapezoidal method.";
                    return;
                }

                (string steps, double[] xPoints, double[] yPoints, double result) = EquationSolver.TrapezoidalRuleWithPlotData(function, a, b, n);

                ResultListView.ItemsSource = steps.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                PlotTrapezoidal(function, a, b, n, xPoints, yPoints);
                FinalRootText.Text = $"Integral Result: {result:F4}";
                PlotStack.Visibility = Visibility.Visible;
            }

            else if (method == "Simpson's 1/3")
            {
                if (!int.TryParse(IntegrationDivisionsInput.Text, out int divisions) || divisions <= 0 || divisions % 2 != 0)
                {
                    FinalRootText.Text = "Error: Invalid number of divisions for Simpson's 1/3 Rule. Must be a positive even integer.";
                    return;
                }

                string function = IntegrationFunctionInput.Text;
                double a = double.Parse(IntegrationLowerBoundInput.Text);
                double b = double.Parse(IntegrationUpperBoundInput.Text);
                (string steps, double[] xPoints, double[] yPoints, double result) = EquationSolver.SimpsonsRuleWithPlotData(function, a, b, divisions);

                string[] formattedSteps = steps.Split('\n');
                foreach (var step in formattedSteps)
                {
                    if (!string.IsNullOrWhiteSpace(step))
                    {
                        ResultListView.Items.Add(step);
                    }
                }

                PlotSimpsons(function, a, b, divisions, xPoints, yPoints);
                PlotStack.Visibility = Visibility.Visible;
                FinalRootText.Text = $"Integral Result: {result:F4}"; // Display the result of the integration
            }

            else if (method == "Euler")
            {
                if (string.IsNullOrEmpty(DifferentialEquationInput.Text) ||
                    string.IsNullOrEmpty(InitialXInput.Text) ||
                    string.IsNullOrEmpty(InitialYInput.Text) ||
                    string.IsNullOrEmpty(EndXInput.Text) ||
                    string.IsNullOrEmpty(StepSizeInput.Text))
                {
                    FinalRootText.Text = "Please fill in all the Euler method parameters.";
                    return;
                }

                if (!double.TryParse(InitialXInput.Text, out double initialX))
                {
                    FinalRootText.Text = "Invalid input for initial x (x?).";
                    return;
                }
                if (!double.TryParse(InitialYInput.Text, out double initialY))
                {
                    FinalRootText.Text = "Invalid input for initial y (y?).";
                    return;
                }
                if (!double.TryParse(EndXInput.Text, out double endX))
                {
                    FinalRootText.Text = "Invalid input for end x.";
                    return;
                }
                if (!double.TryParse(StepSizeInput.Text, out double stepSize))
                {
                    FinalRootText.Text = "Invalid input for step size (h).";
                    return;
                }

                List<StepResult> stepsList = new List<StepResult>(); // Not directly used in Euler, but kept for consistency if you want to extend
                try
                {
                    var eulerResult = EquationSolver.Euler(DifferentialEquationInput.Text, initialX, initialY, endX, stepSize);

                    FinalRootText.Text = $"Approximate y({endX:F4}) = {eulerResult.lastY:F4}";
                    ResultListView.ItemsSource = eulerResult.steps.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line));
                    // Update the method call to include the missing arguments
                    PlotEulerWithComparison(
                        DifferentialEquationInput.Text, // Pass the differential equation as a string
                        x => EquationSolver.EvaluateFunction(DifferentialEquationInput.Text, x), // Pass a function for exact solution
                        eulerResult.xPoints, // Pass the xPointsEuler array
                        eulerResult.yPoints  // Pass the yPointsEuler array
                    );

                    PlotStack.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                    ResultListView.ItemsSource = new string[] { ex.Message };
                    PlotStack.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void PlotEulerWithComparison(
            string differentialEquation, // To potentially derive the exact solution if possible
            Func<double, double> exactSolution, // A function that calculates the exact y for a given x
            double[] xPointsEuler,
            double[] yPointsEuler
        )
        {
            // Assuming you have a ScottPlot.WinUI.PlotView control in your XAML named "WinUiPlotView"
            PlotView.Plot.Clear();

            var eulerScatter = PlotView.Plot.Add.Scatter(
                xPointsEuler,
                yPointsEuler,
                color: ScottPlot.Color.FromHex("#008000") // Green color in hex
            );
            eulerScatter.Label = "Euler Approximation";
            eulerScatter.MarkerSize = 5; // Set the marker size here

            // Generate points for the exact solution
            double[] xPointsExact = xPointsEuler; // Use the same x-values for comparison
            double[] yPointsExact = xPointsExact.Select(exactSolution).ToArray();

            PlotViewControl.Plot.Add.Scatter(
                xPointsExact,
                yPointsExact,
                color: ScottPlot.Color.FromHex("#1a1a1a") // Green color in hex
            ).Label = "Euler Exact";

            PlotView.Plot.Title("Euler's Method vs. Exact Solution");
            PlotView.Plot.XLabel("x");
            PlotView.Plot.YLabel("y");
            PlotViewControl.Plot.Legend.IsVisible = true; // Show the legend
            PlotStack.Visibility = Visibility.Collapsed;
            PlotView.Refresh();
            PlotView.Visibility = Visibility.Visible;
        }


        private void PlotTrapezoidal(string function, double a, double b, int n, double[] xPoints, double[] yPoints)
        {
            var plt = PlotView.Plot;
            plt.Clear();

            // Plot the function
            double[] xFine = Enumerable.Range(0, 200).Select(i => a + i * (b - a) / 199).ToArray();
            double[] yFine = xFine.Select(x => EquationSolver.EvaluateFunction(function, x)).ToArray();
            plt.Add.Scatter(xFine, yFine).LegendText = "f(x)";
            plt.Add.Scatter(xFine, yFine).Color = ScottPlot.Color.FromHex("#E10600");


            // Plot the trapezoids
            for (int i = 0; i < n; i++)
            {
                double[] xTrap = { xPoints[i], xPoints[i + 1], xPoints[i + 1], xPoints[i] };
                double[] yTrap = { 0, 0, yPoints[i + 1], yPoints[i] };
                plt.Add.Polygon(xTrap, yTrap).FillColor = ScottPlot.Color.FromHex("#64FF0000");
            }

            // Apply dark theme
            plt.FigureBackground.Color = ScottPlot.Color.FromHex("#1a1a1a");
            plt.DataBackground.Color = ScottPlot.Color.FromHex("#1a1a1a");
            plt.Axes.Color(ScottPlot.Color.FromHex("#d7d7d7"));
            plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#3a3a3a");
            plt.Legend.BackgroundColor = ScottPlot.Color.FromHex("#3a3a3a");
            plt.Legend.FontColor = ScottPlot.Color.FromHex("#e10600");
            plt.Legend.OutlineColor = ScottPlot.Color.FromHex("#e10600");

            // Add title and labels
            plt.Title($"Trapezoidal Rule (n={n})", size: 18);
            plt.XLabel("x", size: 14);
            plt.YLabel("f(x)", size: 14);

            plt.Legend.IsVisible = true;
            PlotView.Visibility = Visibility.Visible;
            PlotView.Refresh();
        }



        private void PlotSimpsons(string function, double a, double b, int n, double[] xPoints, double[] yPoints)
        {
            var plot = PlotView.Plot;
            plot.Clear();

            // Plot the function
            double[] xFine = Enumerable.Range(0, 200).Select(i => a + i * (b - a) / 199).ToArray();
            double[] yFine = xFine.Select(x => EquationSolver.EvaluateFunction(function, x)).ToArray();
            plot.Add.Scatter(xFine, yFine).Label = "f(x)";

            // Plot the points used in Simpson's rule
            var scatter = plot.Add.Scatter(xPoints, yPoints);
            scatter.LegendText = "Simpson's Points";
            scatter.Color = ScottPlot.Color.FromSDColor(System.Drawing.Color.Red);

            plot.Title("Simpson's 1/3 Rule");
            plot.XLabel("x");
            plot.YLabel("f(x)");
            plot.Legend.IsVisible = true;
            PlotView.Visibility = Visibility.Visible;
            PlotView.Refresh();
        }

        private void PlotPolynomialRegression(double[] xValues, double[] yValues, double[] coefficients)
        {
            if (xValues == null || yValues == null || coefficients == null || xValues.Length != yValues.Length || xValues.Length < 2)
            {
                // Handle cases with insufficient or invalid data for plotting
                PlotView.Plot.Clear();
                PlotView.Visibility = Visibility.Collapsed;
                return;
            }

            var plt = PlotView.Plot;
            plt.Clear();

            // Apply dark theme
            plt.FigureBackground.Color = ScottPlot.Color.FromHex("#1a1a1a");
            plt.DataBackground.Color = ScottPlot.Color.FromHex("#1a1a1a");
            plt.Axes.Color(ScottPlot.Color.FromHex("#d7d7d7"));
            plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#3a3a3a");
            plt.Legend.BackgroundColor = ScottPlot.Color.FromHex("#3a3a3a");
            plt.Legend.FontColor = ScottPlot.Color.FromHex("#e10600");
            plt.Legend.OutlineColor = ScottPlot.Color.FromHex("#e10600");

            // Plot the original data points
            var scatter = plt.Add.Scatter(xValues, yValues);
            scatter.Label = "Data Points";
            scatter.Color = ScottPlot.Color.FromSDColor(System.Drawing.Color.White);

            // Generate points for the polynomial curve
            double minX = xValues.Min();
            double maxX = xValues.Max();
            double[] xCurve = LinSpace(minX, maxX, 100); // Generate 100 points for a smooth curve
            double[] yCurve = EvaluatePolynomial(xCurve, coefficients);

            // Plot the polynomial curve
            var line = plt.Add.Scatter(xCurve, yCurve);
            line.LineColor = ScottPlot.Color.FromHex("#e10600");
            line.Label = $"Polynomial (Degree {coefficients.Length - 1})";

            // Add title and labels
            plt.Title($"Polynomial Regression (Degree {coefficients.Length - 1})", size: 18);
            plt.XLabel("x", size: 14); // Removed the 'color' parameter
            plt.YLabel("y", size: 14); // Removed the 'color' parameter

            plt.Legend.IsVisible = true;
            PlotView.Visibility = Visibility.Visible;
            PlotView.Refresh();
        }


        // Helper function to evaluate the polynomial
        private double[] EvaluatePolynomial(double[] x, double[] coefficients)
        {
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = 0;
                for (int j = 0; j < coefficients.Length; j++)
                {
                    y[i] += coefficients[j] * Math.Pow(x[i], j);
                }
            }
            return y;
        }

        // Helper function for creating a linear space of numbers
        private double[] LinSpace(double start, double end, int count)
        {
            double[] result = new double[count];
            if (count < 2)
            {
                if (count == 1) result[0] = start;
                return result;
            }
            double step = (end - start) / (count - 1);
            for (int i = 0; i < count; i++)
            {
                result[i] = start + i * step;
            }
            return result;
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
        private void MainWindow_Loaded(object sender, WindowActivatedEventArgs e)
        {
            AudioHelper.PlayBackgroundMusic("Assets/Audio/bg_music.mp3");
        }

        private void MusicToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (MusicToggle.IsOn)
            {
                AudioHelper.PlayBackgroundMusic("Assets/Audio/bust_it.mp3"); // Re-play if toggled back on
            }
            else
            {
                AudioHelper.StopBackgroundMusic();
            }
        }
    }
}