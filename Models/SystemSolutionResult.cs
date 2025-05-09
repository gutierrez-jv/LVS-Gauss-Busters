using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVS_Gauss_Busters.Models
{
    public class SystemSolutionResult
    {
        public int Iteration { get; set; }
        public List<double> Values { get; set; } = new();
        public List<double> Errors { get; set; } = new(); // For iterative methods like Gauss-Seidel

        public string FormattedValues => string.Join(", ", Values.Select(v => v.ToString("F5")));
        public string FormattedErrors => Errors.Count > 0 ? string.Join(", ", Errors.Select(e => e.ToString("F5"))) : "-";
    }
}

