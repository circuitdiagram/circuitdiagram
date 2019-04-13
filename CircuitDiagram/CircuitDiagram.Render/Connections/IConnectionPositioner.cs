using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Render.Connections
{
    public interface IConnectionPositioner
    {
        IList<ConnectionPoint> PositionConnections(PositionalComponent instance,
                                                   ComponentDescription description,
                                                   LayoutOptions layoutOptions);
    }
}
