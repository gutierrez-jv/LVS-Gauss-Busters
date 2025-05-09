using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVS_Gauss_Busters.Models
{
    public class StepResult
    {
        public int Iteration { get; set; }
        public double Xl { get; set; }
        public double Xr { get; set; }
        public double Xm { get; set; }
        public double Fxl { get; set; }
        public double Fxr { get; set; }
        public double Fxm { get; set; }
        public double X { get; set; }
        public double Fx { get; set; }
        public double Error { get; set; }

        // Add these for display formatting
        public string XlFormatted => Xl.ToString("F5");
        public string XrFormatted => Xr.ToString("F5");
        public string XmFormatted => Xm.ToString("F5");
        public string FxlFormatted => Fxl.ToString("F5");
        public string FxrFormatted => Fxr.ToString("F5");
        public string FxmFormatted => Fxm.ToString("F5");
    }
}
