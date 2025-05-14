using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using LVS_Gauss_Busters.Models;

namespace LVS_Gauss_Busters
{
    public sealed partial class MainWindow : Window
    {
        private int numberOfEquations = 3; // Default value
        private List<TextBox> equationInputFields = new();
        private List<TextBox> initialGuessInputFields = new();

        public MainWindow()
        {
            this.InitializeComponent();
            UpdateInputVisibility();
            GenerateEquationInputFields();
            GenerateInitialGuessInputFields();
        }

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
        }

        private void GenerateEquationInputFields()
        {
            EquationsInputPanel.Children.Clear();
            equationInputFields.Clear();
            for (int i = 0; i < numberOfEquations; i++)
            {
                var equationPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
                for (int j = 0; j < numberOfEquations; j++)
                {
                    equationPanel.Children.Add(new TextBox
                    {
                        Width = 60,
                        PlaceholderText = $"a{i + 1}{GetVariableLetter(j)}",
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DimGray),
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                    });
                }
                equationPanel.Children.Add(new TextBlock { Text = "=", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), VerticalAlignment = VerticalAlignment.Center });
                equationPanel.Children.Add(new TextBox
                {
                    Width = 60,
                    PlaceholderText = $"b{i + 1}",
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DimGray),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
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
                try
                {
                    double[] x = InputXValues(); // Implement a method to read X values
                    double[] y = InputYValues(); // Implement a method to read Y values

                    var (slope, intercept) = EquationSolver.LinearRegression(x, y);
                    FinalRootText.Text = $"y = {slope:F4}x + {intercept:F4}";
                }
                catch (Exception ex)
                {
                    FinalRootText.Text = $"Error: {ex.Message}";
                }
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
    }
}