using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LVS.Methods;

namespace LVS
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            string expression = EquationInput.Text;
            double x0 = double.TryParse(X0Input.Text, out var val1) ? val1 : 0;
            double x1 = double.TryParse(X1Input.Text, out var val2) ? val2 : 0;

            INumericalMethod method = null;

            switch ((MethodSelector.SelectedItem as ComboBoxItem)?.Content?.ToString())
            {
                case "Newton-Raphson":
                    method = new NewtonRaphson();
                    break;
                // Add cases for "Bisection" and "Secant" once implemented
                default:
                    ResultText.Text = "Please select a valid method.";
                    return;
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                ResultText.Text = "Please enter a valid function expression.";
                return;
            }

            string result = method.Solve(expression, x0, x1);
            ResultText.Text = result;
        }
    }
}
