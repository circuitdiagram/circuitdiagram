using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.TypeDescription.Conditions.Parsers
{
    public class ParseContext
    {
        public IDictionary<PropertyName, PropertyValue.Type> PropertyTypes { get; private set; }

        public ParseContext()
        {
            PropertyTypes = new Dictionary<PropertyName, PropertyValue.Type>();
        }
    }
}
