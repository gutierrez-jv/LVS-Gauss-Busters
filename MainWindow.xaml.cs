using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using LVS_Gauss_Busters.Models;


namespace LVS_Gauss_Busters
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            string equation = EquationInput.Text;
            string method = ((ComboBoxItem)MethodSelector.SelectedItem)?.Content?.ToString();

            if (!double.TryParse(X0Input.Text, out double x0))
            {
                FinalRootText.Text = "Invalid input for X?.";
                ResultListView.ItemsSource = null;
                return;
            }

            double x1 = 0;
            if ((method == "Bisection" || method == "Secant") && !double.TryParse(X1Input.Text, out x1))
            {
                FinalRootText.Text = "Invalid input for X?.";
                ResultListView.ItemsSource = null;
                return;
            }

            try
            {
                List<StepResult> steps = new();
                double root = method switch
                {
                    "Bisection" => EquationSolver.Bisection(equation, x0, x1, steps),
                    "Newton-Raphson" => EquationSolver.NewtonRaphson(equation, x0, steps),
                    "Secant" => EquationSolver.Secant(equation, x0, x1, steps),
                    _ => throw new Exception("Unknown method")
                };

                ResultListView.ItemsSource = steps;
                FinalRootText.Text = $"Approximate Root = {root:F5}";
            }
            catch (Exception ex)
            {
                FinalRootText.Text = $"An error occurred: {ex.Message}";
                ResultListView.ItemsSource = null;
            }
        }
    }
}
