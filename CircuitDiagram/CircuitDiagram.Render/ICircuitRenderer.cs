using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;

namespace CircuitDiagram.Render
{
    public interface ICircuitRenderer
    {
        void RenderComponent(PositionalComponent component, IDrawingContext drawingContext, bool ignoreOffset = true);
    }
}
