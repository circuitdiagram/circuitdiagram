using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.TypeDescription.Extensions
{
    public static class PropertyValueExtensions
    {
        public static bool IsTruthy(this PropertyValue value)
        {
            bool isTruthy = false;
            value.Match(s => isTruthy = !string.IsNullOrEmpty(s),
                        n => isTruthy = Math.Abs(n) > 0.0,
                        b => isTruthy = b);

            return isTruthy;
        }
    }
}
