using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Sections
{
    public interface IXmlSection<out T> where T : class
    {
        bool IsAvailable { get; }

        T Value { get; }
    }
}
