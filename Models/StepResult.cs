using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVS_Gauss_Busters.Models
{
    public class StepResult
    {
        public int Iteration { get; set; }     // Iteration number
        public double Xl { get; set; }         // Lower bound (xl)
        public double Xr { get; set; }         // Upper bound (xr)
        public double Xm { get; set; }         // Midpoint (xm)
        public double Fxl { get; set; }        // f(xl)
        public double Fxr { get; set; }        // f(xr)
        public double Fxm { get; set; }        // f(xm)
    }
}
