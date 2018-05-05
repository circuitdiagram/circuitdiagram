using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    public interface IXmlSectionReader
    {
        void ReadSection(XElement element, ComponentDescription description);
    }
}
