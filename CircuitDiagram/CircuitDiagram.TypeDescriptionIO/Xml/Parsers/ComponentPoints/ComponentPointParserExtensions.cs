using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints
{
    public static class ComponentPointParserExtensions
    {
        public static bool TryParse(this IComponentPointParser parser, XAttribute x, XAttribute y, out XmlComponentPoint componentPoint)
        {
            return parser.TryParse(x.Value, y.Value, x.GetFileRange(), y.GetFileRange(), out componentPoint);
        }

        public static bool TryParse(this IComponentPointParser parser, XAttribute location, out XmlComponentPoint componentPoint)
        {
            return parser.TryParse(location.Value, location.GetFileRange(), out componentPoint);
        }
    }
}
