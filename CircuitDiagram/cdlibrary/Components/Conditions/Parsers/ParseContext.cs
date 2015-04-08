using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Conditions.Parsers
{
    public class ParseContext
    {
        public IDictionary<string, PropertyUnionType> PropertyTypes { get; private set; }

        public ParseContext()
        {
            PropertyTypes = new Dictionary<string, PropertyUnionType>();
        }
    }
}
