using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Logging
{
    public static class XmlExtensions
    {
        public static FileRange GetFileRange(this XElement element)
        {
            var line = (IXmlLineInfo)element;
            return new FileRange(line.LineNumber, line.LinePosition, line.LineNumber, line.LinePosition + element.Name.LocalName.Length);
        }

        public static FileRange GetFileRange(this XAttribute attribute)
        {
            var line = (IXmlLineInfo)attribute;
            var start = line.LinePosition + attribute.Name.LocalName.Length + 2;
            return new FileRange(line.LineNumber, line.LinePosition, line.LineNumber, start + attribute.Value.Length + 1);
        }
    }
}
