using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescription.Conditions.Parsers
{
    public interface IConditionParser
    {
        IConditionTreeItem Parse(string input, ParseContext context);
    }
}
