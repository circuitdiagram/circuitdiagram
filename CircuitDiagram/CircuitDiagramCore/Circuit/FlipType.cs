using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Circuit
{
    [Flags]
    public enum FlipType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = 3
    }
}
