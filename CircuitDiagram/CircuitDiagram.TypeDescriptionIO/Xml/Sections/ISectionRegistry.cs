using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Sections
{
    public interface ISectionRegistry
    {
        void RegisterSection<T>(T section);
    }
}
