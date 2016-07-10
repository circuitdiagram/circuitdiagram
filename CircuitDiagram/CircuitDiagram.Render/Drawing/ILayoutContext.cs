using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.Drawing
{
    public interface ILayoutContext
    {
        LayoutOptions Options { get; }

        string GetFormattedVariable(string variableName);
    }
}
