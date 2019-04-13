using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Render.Connections
{
    [Flags]
    public enum ConnectionFlags
    {
        Horizontal = 1,
        Vertical = 2,
        Edge = 4
    }
}
