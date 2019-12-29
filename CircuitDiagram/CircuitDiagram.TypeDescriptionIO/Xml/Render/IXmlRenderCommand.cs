using CircuitDiagram.Drawing;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public interface IXmlRenderCommand : IFlattenable<IRenderCommand>
    {
    }
}
