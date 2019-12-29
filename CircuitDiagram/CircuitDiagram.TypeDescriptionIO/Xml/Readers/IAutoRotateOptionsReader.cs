using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    public interface IAutoRotateOptionsReader
    {
        bool TrySetAutoRotateOptions(XElement element, IAutoRotateRoot target);

        bool TrySetAutoRotateOptions(XElement element, IAutoRotateRoot ancestor, IAutoRotateRoot target);
    }
}
