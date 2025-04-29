using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVS.Methods
{
    public interface INumericalMethod
    {
        string Solve(string functionExpression, double x0, double x1 = 0); // x1 optional for methods that don't need it
    }
}

