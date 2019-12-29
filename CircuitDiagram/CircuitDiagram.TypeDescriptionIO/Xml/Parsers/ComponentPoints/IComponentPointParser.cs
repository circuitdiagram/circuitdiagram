using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints
{
    public interface IComponentPointParser
    {
        bool TryParse(string x, string y, FileRange xRange, FileRange yRange, out XmlComponentPoint componentPoint);

        bool TryParse(string location, FileRange range, out XmlComponentPoint componentPoint);
    }
}
