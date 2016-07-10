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
        public static bool IsEmpty(this PropertyValue value)
        {
            bool empty = false;
            value.Match(s => empty = string.IsNullOrEmpty(s),
                        n => empty = false,
                        b => empty = false);

            return empty;
        }
    }
}
