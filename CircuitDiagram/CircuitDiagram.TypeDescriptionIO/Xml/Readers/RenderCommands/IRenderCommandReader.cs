using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers.RenderCommands
{
    public interface IRenderCommandReader
    {
        bool ReadRenderCommand(XElement element, ComponentDescription description, out IXmlRenderCommand command);
    }
}
