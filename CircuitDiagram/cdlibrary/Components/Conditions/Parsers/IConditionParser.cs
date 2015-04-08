using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Conditions.Parsers
{
    internal interface IConditionParser
    {
        IConditionTreeItem Parse(string input, ParseContext context);
    }
}
