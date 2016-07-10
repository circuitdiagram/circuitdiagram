using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.Drawing
{
    public class LayoutContext : ILayoutContext
    {
        private readonly Func<string, string> formattedVariable;

        public LayoutContext(LayoutOptions options,
                             Func<string, string> formattedVariable)
        {
            this.formattedVariable = formattedVariable;
            Options = options;
        }

        public LayoutOptions Options { get; }

        public string GetFormattedVariable(string variableName)
        {
            return formattedVariable(variableName);
        }
    }
}
