using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints
{
    public interface IComponentPointParser
    {
        bool TryParse(string x, string y, IXmlLineInfo xLine, IXmlLineInfo yLine, out ComponentPoint componentPoint);

        bool TryParse(string location, IXmlLineInfo line, out ComponentPoint componentPoint);
    }
}
