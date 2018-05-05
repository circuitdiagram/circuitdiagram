using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints
{
    public static class ComponentPointParserExtensions
    {
        public static bool TryParse(this IComponentPointParser parser, XAttribute x, XAttribute y, out ComponentPoint componentPoint)
        {
            return parser.TryParse(x.Value, y.Value, x, y, out componentPoint);
        }

        public static bool TryParse(this IComponentPointParser parser, XAttribute location, out ComponentPoint componentPoint)
        {
            return parser.TryParse(location.Value, location, out componentPoint);
        }
    }
}
